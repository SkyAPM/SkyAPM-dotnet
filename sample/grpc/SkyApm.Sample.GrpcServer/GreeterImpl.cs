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
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Core;
using GrpcGreeter;

namespace SkyApm.Sample.GrpcServer
{
    public class GreeterImpl : Greeter.GreeterBase
    {
        // Server side handler of the SayHello RPC
        public override async Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            await Task.Delay(150);
            var httpClient = new HttpClient();
            var result = await httpClient.GetAsync("http://www.baidu.com");
            Console.WriteLine(result.Content.Headers);
            return new HelloReply { Message = "Hello " + request.Name };
        }

        public override Task<HelloReply> SayHelloWithException(HelloRequest request, ServerCallContext context)
        {
            throw new Exception("grpc server throw exception ！！！");
        }

        public override async Task<HelloReply> SayHelloByClientStreaming(IAsyncStreamReader<HelloRequest> requestStream, ServerCallContext context)
        {
            var names = new List<string>();
            while (await requestStream.MoveNext())
            {
                names.Add(requestStream.Current.Name);
            }
            return new HelloReply { Message = string.Join(",", names) };
        }

        public override async Task SayHelloByServerStreaming(HelloRequest request, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
        {
            var count = 10;
            while (count > 0)
            {
                count--;
                await responseStream.WriteAsync(new HelloReply
                {
                    Message = $"{request.Name}-{count}"
                });
            }
        }

        public override async Task SayHelloByDuplexStreaming(IAsyncStreamReader<HelloRequest> requestStream, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
        {
            var httpClient = new HttpClient();
            var result = await httpClient.GetAsync("http://www.baidu.com");
            Console.WriteLine(result.Content.Headers);

            while (await requestStream.MoveNext())
            {
                await responseStream.WriteAsync(new HelloReply
                {
                    Message = requestStream.Current.Name
                });
            }
        }
    }
}
