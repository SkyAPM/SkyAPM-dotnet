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
using System.Threading.Tasks;

namespace SkyApm.Diagnostics.Grpc.Server
{
    public class ServerDiagnosticInterceptor : Interceptor
    {
        private readonly ServerDiagnosticProcessor _processor;

        public ServerDiagnosticInterceptor(ServerDiagnosticProcessor processor)
        {
            _processor = processor;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
             _processor.BeginRequest(context);

            return await Handler(context, async () =>
            {
                return await continuation(request, context);
            });
        }

        public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, ServerCallContext context, ClientStreamingServerMethod<TRequest, TResponse> continuation)
        {
            return await Handler(context, async () =>
            {
                return await continuation(requestStream, context);
            });
        }

        public override async Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            await Handler(context, async () =>
            {
                await continuation(request, responseStream, context);
            });
        }

        public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, DuplexStreamingServerMethod<TRequest, TResponse> continuation)
        {
            await Handler(context, async () =>
            {
                await continuation(requestStream, responseStream, context);
            });
        }

        private async Task Handler(ServerCallContext context, Func<Task> func)
        {
            _processor.BeginRequest(context);
            try
            {
                await func();
                _processor.EndRequest(context);
            }
            catch (Exception ex)
            {
                _processor.DiagnosticUnhandledException(ex);
                throw;
            }
        }

        private async Task<TResponse> Handler<TResponse>(ServerCallContext context, Func<Task<TResponse>> func)
            where TResponse : class
        {
            _processor.BeginRequest(context);
            try
            {
                var response = await func();
                _processor.EndRequest(context);
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
