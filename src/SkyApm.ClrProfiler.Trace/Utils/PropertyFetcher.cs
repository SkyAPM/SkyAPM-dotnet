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
 
using System;
using System.Reflection;

namespace SkyApm.ClrProfiler.Trace.Utils
{
    public class PropertyFetcher
    {
        private readonly string _propertyName;
        private PropertyFetch _fetchForExpectedType;

        private static readonly BindingFlags DefaultBindingFlags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Make a new PropertyFetcher for a property named 'propertyName'.
        /// </summary>
        public PropertyFetcher(string propertyName)
        {
            _propertyName = propertyName;
        }

        /// <summary>
        /// Given an object fetch the property that this PropertySpec represents.
        /// </summary>
        public object Fetch(object obj)
        {
            Type objType = obj.GetType();
            if (_fetchForExpectedType == null)
            {
                TypeInfo typeInfo = objType.GetTypeInfo();
                PropertyInfo propertyInfo = typeInfo.GetProperty(_propertyName, DefaultBindingFlags);
                _fetchForExpectedType = PropertyFetch.FetcherForProperty(propertyInfo);
            }
            return _fetchForExpectedType.Fetch(obj);
        }


        /// <summary>
        /// PropertyFetch is a helper class. It takes a PropertyInfo and then knows how
        /// to efficiently fetch that property from a .NET object (See Fetch method).
        /// It hides some slightly complex generic code.  
        /// </summary>
        private class PropertyFetch
        {
            /// <summary>
            /// Create a property fetcher from a .NET Reflection PropertyInfo class that
            /// represents a property of a particular type.
            /// </summary>
            public static PropertyFetch FetcherForProperty(PropertyInfo propertyInfo)
            {
                if (propertyInfo == null)
                    return new PropertyFetch();     // returns null on any fetch.

                Type typedPropertyFetcher = typeof(TypedFetchProperty<,>);
                Type instantiatedTypedPropertyFetcher = typedPropertyFetcher.GetTypeInfo().MakeGenericType(
                    propertyInfo.DeclaringType, propertyInfo.PropertyType);

                return (PropertyFetch)Activator.CreateInstance(instantiatedTypedPropertyFetcher, propertyInfo);
            }

            /// <summary>
            /// Given an object, fetch the property that this propertyFech represents.
            /// </summary>
            public virtual object Fetch(object obj)
            {
                return null;
            }

            private class TypedFetchProperty<TObject, TProperty> : PropertyFetch
            {
                public TypedFetchProperty(PropertyInfo property)
                {
                    _propertyFetch = (Func<TObject, TProperty>)property.GetMethod.CreateDelegate(typeof(Func<TObject, TProperty>));
                }
                public override object Fetch(object obj)
                {
                    return _propertyFetch((TObject)obj);
                }
                private readonly Func<TObject, TProperty> _propertyFetch;
            }
        }
    }
}

