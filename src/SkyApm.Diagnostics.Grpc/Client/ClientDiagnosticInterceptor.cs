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

using Grpc.Core;
using Grpc.Core.Interceptors;
using System;

namespace SkyApm.Diagnostics.Grpc.Client
{
    public class ClientDiagnosticInterceptor : Interceptor
    {
        private readonly ClientDiagnosticProcessor _processor;

        public ClientDiagnosticInterceptor(ClientDiagnosticProcessor processor)
        {
            _processor = processor;
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            return Call(context, (newContext) =>
            {
                return continuation(request, newContext);
            });
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            return Call(context, (newContext) =>
            {
                var response = continuation(request, newContext);
                var responseAsync = response.ResponseAsync.ContinueWith(r =>
                {
                    try
                    {
                        _processor.EndRequest();
                        return r.Result;
                    }
                    catch (Exception ex)
                    {
                        _processor.DiagnosticUnhandledException(ex);
                        throw;
                    }
                });
                return new AsyncUnaryCall<TResponse>(responseAsync, response.ResponseHeadersAsync, response.GetStatus, response.GetTrailers, response.Dispose);
            });
        }

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context, AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            return Call(context, (newContext) =>
            {
                return continuation(newContext);
            });
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            return Call(context, (newContext) =>
            {
                return continuation(request, newContext);
            });
        }

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context, AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            return Call(context, (newContext) =>
            {
                return continuation(newContext);
            });
        }

        public T Call<T, TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context, Func<ClientInterceptorContext<TRequest, TResponse>, T> func)
            where TRequest : class
            where TResponse : class
        {
            var metadata = _processor.BeginRequest(context);
            try
            {
                var options = context.Options.WithHeaders(metadata);
                var newContext = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, options);
                var response = func(newContext);
                _processor.EndRequest();
                return response;
            }
            catch (Exception ex)
            {
                _processor.DiagnosticUnhandledException(ex);
                throw;
            }
        }
    }
}