using Grpc.Core;
using GrpcGreeter;
using System;
using System.Net.Http;
using System.Threading.Tasks;

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
    }
}