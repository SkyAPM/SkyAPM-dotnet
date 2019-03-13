 /*
 * Licensed to the SkyAPM under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The SkyAPM licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */
 
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace SkyApm.ClrProfiler.Trace.HttpWebRequest
{
    internal sealed class HttpWebRequestDiagnostic
    {
        public static readonly HttpWebRequestDiagnostic Instance = new HttpWebRequestDiagnostic();

        // code from https://github.com/dotnet/corefx/blob/master/src/System.Diagnostics.DiagnosticSource/src/System/Diagnostics/HttpHandlerDiagnosticListener.cs
        // Fields for controlling initialization of the HttpHandlerDiagnosticListener singleton
        private bool initialized = false;

        // Fields for reflection
        private static FieldInfo s_connectionGroupListField;
        private static Type s_connectionGroupType;
        private static FieldInfo s_connectionListField;
        private static Type s_connectionType;
        private static FieldInfo s_writeListField;
        private static Func<System.Net.HttpWebRequest, HttpWebResponse> s_httpResponseAccessor;
        private static Func<System.Net.HttpWebRequest, int> s_autoRedirectsAccessor;
        private static Func<System.Net.HttpWebRequest, object> s_coreResponseAccessor;
        private static Func<object, HttpStatusCode> s_coreStatusCodeAccessor;
        private static Func<object, WebHeaderCollection> s_coreHeadersAccessor;
        private static Type s_coreResponseDataType;

        private ITracingContext _tracingContext;
        static readonly ConditionalWeakTable<object, GCNotice> gcNotificationMap = new ConditionalWeakTable<object, GCNotice>();
        private static readonly Exception HttpException = new Exception("Http Connection Error");

        class GCNotice
        {
            private object _request;
            private readonly SegmentContext _context;
            private ITracingContext _tracingContext;

            public GCNotice(object request, SegmentContext context, ITracingContext tracingContext)
            {
                _request = request;
                _context = context;
                _tracingContext = tracingContext;
            }

            public SegmentContext GetSegmentContext()
            {
                return _context;
            }

            ~GCNotice()
            {
                try
                {
                    object trackedObject = _request;
                    _request = null;
                    gcNotificationMap.Remove(trackedObject);
                    _context.Span.ErrorOccurred(HttpException);
                    _tracingContext?.Release(_context);
                }
                catch
                {
                    // ignored
                }
            }
        }

        public void Initialize(ITracingContext tracer)
        {
            if (!this.initialized)
            {
                lock (this)
                {
                    if (!this.initialized)
                    {
                        _tracingContext = _tracingContext ?? tracer;
                        try
                        {
                            // This flag makes sure we only do this once. Even if we failed to initialize in an
                            // earlier time, we should not retry because this initialization is not cheap and
                            // the likelihood it will succeed the second time is very small.
                            this.initialized = true;

                            PrepareReflectionObjects();
                            PerformInjection();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Trace.WriteLine(ex);
                        }
                    }
                }
            }
        }
        private static readonly string RequestIdHeaderName = "Empty";

        private void RaiseRequestEvent(System.Net.HttpWebRequest request)
        {
            if (request.Headers.Get(RequestIdHeaderName) != null)
            {
                // this request was instrumented by previous RaiseRequestEvent
                return;
            }
            request.Headers[RequestIdHeaderName] = string.Empty;

            var operationName = request.RequestUri.ToString();
            var networkAddress = $"{request.RequestUri.Host}:{request.RequestUri.Port}";
            var context = _tracingContext.CreateExitSegmentContext(operationName, networkAddress,
              new CarrierHeaderCollection(request.Headers));

            context.Span.SpanLayer = SpanLayer.HTTP;
            context.Span.Component = Common.Components.HTTPCLIENT;
            context.Span.AddTag(Common.Tags.URL, request.RequestUri.ToString());
            context.Span.AddTag(Common.Tags.PATH, request.RequestUri.PathAndQuery);
            context.Span.AddTag(Common.Tags.HTTP_METHOD, request.Method.ToString());

            if (!gcNotificationMap.TryGetValue(request, out var gcNotice))
            {
                gcNotificationMap.Add(request, new GCNotice(request, context, _tracingContext));
            }
        }

        private class CarrierHeaderCollection : ICarrierHeaderCollection
        {
            private readonly WebHeaderCollection _headers;

            public CarrierHeaderCollection(WebHeaderCollection headers)
            {
                _headers = headers;
            }

            public void Add(string key, string value)
            {
                _headers.Add(key, value);
            }

            public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private void RaiseResponseEvent(System.Net.HttpWebRequest request, HttpWebResponse response)
        {
            // Response event could be received several times for the same request in case it was redirected
            // IsLastResponse checks if response is the last one (no more redirects will happen)
            // based on response StatusCode and number or redirects done so far
            if (request.Headers[RequestIdHeaderName] != null && IsLastResponse(request, response.StatusCode))
            {
                if (gcNotificationMap.TryGetValue(request, out var gcNotice))
                {
                    GC.SuppressFinalize(gcNotice);

                    Debug.Assert(gcNotificationMap.Remove(request));
                    var context = gcNotice.GetSegmentContext();
                    if (context == null)
                    {
                        return;
                    }

                    var statusCode = (int)response.StatusCode;
                    if (statusCode >= 400)
                    {
                        context.Span.ErrorOccurred();
                    }

                    context.Span.AddTag(Common.Tags.STATUS_CODE, statusCode);
                    _tracingContext.Release(context);
                }
            }
        }

        private void RaiseResponseEvent(System.Net.HttpWebRequest request, HttpStatusCode statusCode, WebHeaderCollection headers)
        {
            // Response event could be received several times for the same request in case it was redirected
            // IsLastResponse checks if response is the last one (no more redirects will happen)
            // based on response StatusCode and number or redirects done so far
            if (request.Headers[RequestIdHeaderName] != null && IsLastResponse(request, statusCode))
            {
                if (gcNotificationMap.TryGetValue(request, out var gcNotice))
                {
                    GC.SuppressFinalize(gcNotice);

                    Debug.Assert(gcNotificationMap.Remove(request));
                    var context = gcNotice.GetSegmentContext();
                    if (context == null)
                    {
                        return;
                    }

                    var statusCode0 = (int)statusCode;
                    if (statusCode0 >= 400)
                    {
                        context.Span.ErrorOccurred();
                    }

                    context.Span.AddTag(Common.Tags.STATUS_CODE, statusCode0);
                    _tracingContext.Release(context);
                }
            }
        }

        #region private methods

        private bool IsLastResponse(System.Net.HttpWebRequest request, HttpStatusCode statusCode)
        {
            if (request.AllowAutoRedirect)
            {
                if (statusCode == HttpStatusCode.Ambiguous ||  // 300
                    statusCode == HttpStatusCode.Moved ||  // 301
                    statusCode == HttpStatusCode.Redirect ||  // 302
                    statusCode == HttpStatusCode.RedirectMethod ||  // 303
                    statusCode == HttpStatusCode.RedirectKeepVerb ||  // 307
                    (int)statusCode == 308) // 308 Permanent Redirect is not in netfx yet, and so has to be specified this way.
                {
                    return s_autoRedirectsAccessor(request) >= request.MaximumAutomaticRedirections;
                }
            }

            return true;
        }

        private static void PrepareReflectionObjects()
        {
            // At any point, if the operation failed, it should just throw. The caller should catch all exceptions and swallow.

            // First step: Get all the reflection objects we will ever need.
            Assembly systemNetHttpAssembly = typeof(ServicePoint).Assembly;
            s_connectionGroupListField = typeof(ServicePoint).GetField("m_ConnectionGroupList", BindingFlags.Instance | BindingFlags.NonPublic);
            s_connectionGroupType = systemNetHttpAssembly?.GetType("System.Net.ConnectionGroup");
            s_connectionListField = s_connectionGroupType?.GetField("m_ConnectionList", BindingFlags.Instance | BindingFlags.NonPublic);
            s_connectionType = systemNetHttpAssembly?.GetType("System.Net.Connection");
            s_writeListField = s_connectionType?.GetField("m_WriteList", BindingFlags.Instance | BindingFlags.NonPublic);

            s_httpResponseAccessor = CreateFieldGetter<System.Net.HttpWebRequest, HttpWebResponse>("_HttpResponse", BindingFlags.NonPublic | BindingFlags.Instance);
            s_autoRedirectsAccessor = CreateFieldGetter<System.Net.HttpWebRequest, int>("_AutoRedirects", BindingFlags.NonPublic | BindingFlags.Instance);
            s_coreResponseAccessor = CreateFieldGetter<System.Net.HttpWebRequest, object>("_CoreResponse", BindingFlags.NonPublic | BindingFlags.Instance);

            s_coreResponseDataType = systemNetHttpAssembly?.GetType("System.Net.CoreResponseData");
            if (s_coreResponseDataType != null)
            {
                s_coreStatusCodeAccessor = CreateFieldGetter<HttpStatusCode>(s_coreResponseDataType, "m_StatusCode", BindingFlags.Public | BindingFlags.Instance);
                s_coreHeadersAccessor = CreateFieldGetter<WebHeaderCollection>(s_coreResponseDataType, "m_ResponseHeaders", BindingFlags.Public | BindingFlags.Instance);
            }
            // Double checking to make sure we have all the pieces initialized
            if (s_connectionGroupListField == null ||
                s_connectionGroupType == null ||
                s_connectionListField == null ||
                s_connectionType == null ||
                s_writeListField == null ||
                s_httpResponseAccessor == null ||
                s_autoRedirectsAccessor == null ||
                s_coreResponseDataType == null ||
                s_coreStatusCodeAccessor == null ||
                s_coreHeadersAccessor == null)
            {
                // If anything went wrong here, just return false. There is nothing we can do.
                throw new InvalidOperationException("Unable to initialize all required reflection objects");
            }
        }

        private static void PerformInjection()
        {
            FieldInfo servicePointTableField = typeof(ServicePointManager).GetField("s_ServicePointTable", BindingFlags.Static | BindingFlags.NonPublic);
            if (servicePointTableField == null)
            {
                // If anything went wrong here, just return false. There is nothing we can do.
                throw new InvalidOperationException("Unable to access the ServicePointTable field");
            }

            Hashtable originalTable = servicePointTableField.GetValue(null) as Hashtable;
            ServicePointHashtable newTable = new ServicePointHashtable(originalTable ?? new Hashtable());

            servicePointTableField.SetValue(null, newTable);
        }

        private static Func<TClass, TField> CreateFieldGetter<TClass, TField>(string fieldName, BindingFlags flags) where TClass : class
        {
            FieldInfo field = typeof(TClass).GetField(fieldName, flags);
            if (field != null)
            {
                string methodName = field.ReflectedType.FullName + ".get_" + field.Name;
                DynamicMethod getterMethod = new DynamicMethod(methodName, typeof(TField), new[] { typeof(TClass) }, true);
                ILGenerator generator = getterMethod.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, field);
                generator.Emit(OpCodes.Ret);
                return (Func<TClass, TField>)getterMethod.CreateDelegate(typeof(Func<TClass, TField>));
            }

            return null;
        }


        /// <summary>
        /// Creates getter for a field defined in private or internal type
        /// repesented with classType variable
        /// </summary>
        private static Func<object, TField> CreateFieldGetter<TField>(Type classType, string fieldName, BindingFlags flags)
        {
            FieldInfo field = classType.GetField(fieldName, flags);
            if (field != null)
            {
                string methodName = classType.FullName + ".get_" + field.Name;
                DynamicMethod getterMethod = new DynamicMethod(methodName, typeof(TField), new[] { typeof(object) }, true);
                ILGenerator generator = getterMethod.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Castclass, classType);
                generator.Emit(OpCodes.Ldfld, field);
                generator.Emit(OpCodes.Ret);

                return (Func<object, TField>)getterMethod.CreateDelegate(typeof(Func<object, TField>));
            }

            return null;
        }

        #endregion


        #region private helper classes

        private class HashtableWrapper : Hashtable, IEnumerable
        {
            protected Hashtable _table;
            public override int Count
            {
                get
                {
                    return this._table.Count;
                }
            }
            public override bool IsReadOnly
            {
                get
                {
                    return this._table.IsReadOnly;
                }
            }
            public override bool IsFixedSize
            {
                get
                {
                    return this._table.IsFixedSize;
                }
            }
            public override bool IsSynchronized
            {
                get
                {
                    return this._table.IsSynchronized;
                }
            }
            public override object this[object key]
            {
                get
                {
                    return this._table[key];
                }
                set
                {
                    this._table[key] = value;
                }
            }
            public override object SyncRoot
            {
                get
                {
                    return this._table.SyncRoot;
                }
            }
            public override ICollection Keys
            {
                get
                {
                    return this._table.Keys;
                }
            }
            public override ICollection Values
            {
                get
                {
                    return this._table.Values;
                }
            }
            internal HashtableWrapper(Hashtable table) : base()
            {
                this._table = table;
            }
            public override void Add(object key, object value)
            {
                this._table.Add(key, value);
            }
            public override void Clear()
            {
                this._table.Clear();
            }
            public override bool Contains(object key)
            {
                return this._table.Contains(key);
            }
            public override bool ContainsKey(object key)
            {
                return this._table.ContainsKey(key);
            }
            public override bool ContainsValue(object key)
            {
                return this._table.ContainsValue(key);
            }
            public override void CopyTo(Array array, int arrayIndex)
            {
                this._table.CopyTo(array, arrayIndex);
            }
            public override object Clone()
            {
                return new HashtableWrapper((Hashtable)this._table.Clone());
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this._table.GetEnumerator();
            }
            public override IDictionaryEnumerator GetEnumerator()
            {
                return this._table.GetEnumerator();
            }
            public override void Remove(object key)
            {
                this._table.Remove(key);
            }
        }

        /// <summary>
        /// Helper class used for ServicePointManager.s_ServicePointTable. The goal here is to
        /// intercept each new ServicePoint object being added to ServicePointManager.s_ServicePointTable
        /// and replace its ConnectionGroupList hashtable field.
        /// </summary>
        private sealed class ServicePointHashtable : HashtableWrapper
        {
            public ServicePointHashtable(Hashtable table) : base(table)
            {
            }

            public override object this[object key]
            {
                get
                {
                    return base[key];
                }
                set
                {
                    WeakReference weakRef = value as WeakReference;
                    if (weakRef != null && weakRef.IsAlive)
                    {
                        ServicePoint servicePoint = weakRef.Target as ServicePoint;
                        if (servicePoint != null)
                        {
                            // Replace the ConnectionGroup hashtable inside this ServicePoint object,
                            // which allows us to intercept each new ConnectionGroup object added under
                            // this ServicePoint.
                            Hashtable originalTable = s_connectionGroupListField.GetValue(servicePoint) as Hashtable;
                            ConnectionGroupHashtable newTable = new ConnectionGroupHashtable(originalTable ?? new Hashtable());

                            s_connectionGroupListField.SetValue(servicePoint, newTable);
                        }
                    }

                    base[key] = value;
                }
            }
        }

        /// <summary>
        /// Helper class used for ServicePoint.m_ConnectionGroupList. The goal here is to
        /// intercept each new ConnectionGroup object being added to ServicePoint.m_ConnectionGroupList
        /// and replace its m_ConnectionList arraylist field.
        /// </summary>
        private sealed class ConnectionGroupHashtable : HashtableWrapper
        {
            public ConnectionGroupHashtable(Hashtable table) : base(table)
            {
            }

            public override object this[object key]
            {
                get
                {
                    return base[key];
                }
                set
                {
                    if (s_connectionGroupType.IsInstanceOfType(value))
                    {
                        // Replace the Connection arraylist inside this ConnectionGroup object,
                        // which allows us to intercept each new Connection object added under
                        // this ConnectionGroup.
                        ArrayList originalArrayList = s_connectionListField.GetValue(value) as ArrayList;
                        ConnectionArrayList newArrayList = new ConnectionArrayList(originalArrayList ?? new ArrayList());

                        s_connectionListField.SetValue(value, newArrayList);
                    }

                    base[key] = value;
                }
            }
        }

        /// <summary>
        /// Helper class used to wrap the array list object. This class itself doesn't actually
        /// have the array elements, but rather access another array list that's given at 
        /// construction time.
        /// </summary>
        private class ArrayListWrapper : ArrayList
        {
            private ArrayList _list;

            public override int Capacity
            {
                get
                {
                    return this._list.Capacity;
                }
                set
                {
                    this._list.Capacity = value;
                }
            }
            public override int Count
            {
                get
                {
                    return this._list.Count;
                }
            }
            public override bool IsReadOnly
            {
                get
                {
                    return this._list.IsReadOnly;
                }
            }
            public override bool IsFixedSize
            {
                get
                {
                    return this._list.IsFixedSize;
                }
            }
            public override bool IsSynchronized
            {
                get
                {
                    return this._list.IsSynchronized;
                }
            }
            public override object this[int index]
            {
                get
                {
                    return this._list[index];
                }
                set
                {
                    this._list[index] = value;
                }
            }
            public override object SyncRoot
            {
                get
                {
                    return this._list.SyncRoot;
                }
            }
            internal ArrayListWrapper(ArrayList list) : base()
            {
                this._list = list;
            }
            public override int Add(object value)
            {
                return this._list.Add(value);
            }
            public override void AddRange(ICollection c)
            {
                this._list.AddRange(c);
            }
            public override int BinarySearch(object value)
            {
                return this._list.BinarySearch(value);
            }
            public override int BinarySearch(object value, IComparer comparer)
            {
                return this._list.BinarySearch(value, comparer);
            }
            public override int BinarySearch(int index, int count, object value, IComparer comparer)
            {
                return this._list.BinarySearch(index, count, value, comparer);
            }
            public override void Clear()
            {
                this._list.Clear();
            }
            public override object Clone()
            {
                return new ArrayListWrapper((ArrayList)this._list.Clone());
            }
            public override bool Contains(object item)
            {
                return this._list.Contains(item);
            }
            public override void CopyTo(Array array)
            {
                this._list.CopyTo(array);
            }
            public override void CopyTo(Array array, int index)
            {
                this._list.CopyTo(array, index);
            }
            public override void CopyTo(int index, Array array, int arrayIndex, int count)
            {
                this._list.CopyTo(index, array, arrayIndex, count);
            }
            public override IEnumerator GetEnumerator()
            {
                return this._list.GetEnumerator();
            }
            public override IEnumerator GetEnumerator(int index, int count)
            {
                return this._list.GetEnumerator(index, count);
            }
            public override int IndexOf(object value)
            {
                return this._list.IndexOf(value);
            }
            public override int IndexOf(object value, int startIndex)
            {
                return this._list.IndexOf(value, startIndex);
            }
            public override int IndexOf(object value, int startIndex, int count)
            {
                return this._list.IndexOf(value, startIndex, count);
            }
            public override void Insert(int index, object value)
            {
                this._list.Insert(index, value);
            }
            public override void InsertRange(int index, ICollection c)
            {
                this._list.InsertRange(index, c);
            }
            public override int LastIndexOf(object value)
            {
                return this._list.LastIndexOf(value);
            }
            public override int LastIndexOf(object value, int startIndex)
            {
                return this._list.LastIndexOf(value, startIndex);
            }
            public override int LastIndexOf(object value, int startIndex, int count)
            {
                return this._list.LastIndexOf(value, startIndex, count);
            }
            public override void Remove(object value)
            {
                this._list.Remove(value);
            }
            public override void RemoveAt(int index)
            {
                this._list.RemoveAt(index);
            }
            public override void RemoveRange(int index, int count)
            {
                this._list.RemoveRange(index, count);
            }
            public override void Reverse(int index, int count)
            {
                this._list.Reverse(index, count);
            }
            public override void SetRange(int index, ICollection c)
            {
                this._list.SetRange(index, c);
            }
            public override ArrayList GetRange(int index, int count)
            {
                return this._list.GetRange(index, count);
            }
            public override void Sort()
            {
                this._list.Sort();
            }
            public override void Sort(IComparer comparer)
            {
                this._list.Sort(comparer);
            }
            public override void Sort(int index, int count, IComparer comparer)
            {
                this._list.Sort(index, count, comparer);
            }
            public override object[] ToArray()
            {
                return this._list.ToArray();
            }
            public override Array ToArray(Type type)
            {
                return this._list.ToArray(type);
            }
            public override void TrimToSize()
            {
                this._list.TrimToSize();
            }
        }

        /// <summary>
        /// Helper class used for ConnectionGroup.m_ConnectionList. The goal here is to
        /// intercept each new Connection object being added to ConnectionGroup.m_ConnectionList
        /// and replace its m_WriteList arraylist field.
        /// </summary>
        private sealed class ConnectionArrayList : ArrayListWrapper
        {
            public ConnectionArrayList(ArrayList list) : base(list)
            {
            }

            public override int Add(object value)
            {
                if (s_connectionType.IsInstanceOfType(value))
                {
                    // Replace the HttpWebRequest arraylist inside this Connection object,
                    // which allows us to intercept each new HttpWebRequest object added under
                    // this Connection.
                    ArrayList originalArrayList = s_writeListField.GetValue(value) as ArrayList;
                    HttpWebRequestArrayList newArrayList = new HttpWebRequestArrayList(originalArrayList ?? new ArrayList());

                    s_writeListField.SetValue(value, newArrayList);
                }

                return base.Add(value);
            }
        }

        /// <summary>
        /// Helper class used for Connection.m_WriteList. The goal here is to
        /// intercept all new HttpWebRequest objects being added to Connection.m_WriteList
        /// and notify the listener about the HttpWebRequest that's about to send a request.
        /// It also intercepts all HttpWebRequest objects that are about to get removed from
        /// Connection.m_WriteList as they have completed the request.
        /// </summary>
        private sealed class HttpWebRequestArrayList : ArrayListWrapper
        {
            public HttpWebRequestArrayList(ArrayList list) : base(list)
            {
            }

            public override int Add(object value)
            {
                System.Net.HttpWebRequest request = value as System.Net.HttpWebRequest;
                if (request != null)
                {
                    Instance.RaiseRequestEvent(request);
                }

                return base.Add(value);
            }

            public override void RemoveAt(int index)
            {
                System.Net.HttpWebRequest request = base[index] as System.Net.HttpWebRequest;
                if (request != null)
                {
                    HttpWebResponse response = s_httpResponseAccessor(request);
                    if (response != null)
                    {
                        Instance.RaiseResponseEvent(request, response);
                    }
                    else
                    {
                        // In case reponse content length is 0 and request is async, 
                        // we won't have a HttpWebResponse set on request object when this method is called
                        // http://referencesource.microsoft.com/#System/net/System/Net/HttpWebResponse.cs,525

                        // But we there will be CoreResponseData object that is either exception 
                        // or the internal HTTP reponse representation having status, content and headers

                        var coreResponse = s_coreResponseAccessor(request);
                        if (coreResponse != null && s_coreResponseDataType.IsInstanceOfType(coreResponse))
                        {
                            HttpStatusCode status = s_coreStatusCodeAccessor(coreResponse);
                            WebHeaderCollection headers = s_coreHeadersAccessor(coreResponse);

                            // Manual creation of HttpWebResponse here is not possible as this method is eventually called from the 
                            // HttpWebResponse ctor. So we will send Stop event with the Status and Headers payload
                            // to notify listeners about response;
                            // We use two different names for Stop events since one event with payload type that varies creates
                            // complications for efficient payload parsing and is not supported by DiagnosicSource helper 
                            // libraries (e.g. Microsoft.Extensions.DiagnosticAdapter)

                            Instance.RaiseResponseEvent(request, status, headers);
                        }
                    }
                }

                base.RemoveAt(index);
            }
        }

        #endregion
    }
}

