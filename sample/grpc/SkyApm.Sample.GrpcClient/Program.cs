using Grpc.Core;
using Grpc.Core.Interceptors;
using GrpcGreeter;
using System;

namespace SkyApm.Sample.GrpcClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Channel channel = new Channel("127.0.0.1:12345", ChannelCredentials.Insecure);
            var invoker = channel.Intercept(new LoggerInterceptor(null));

            var client = new Greeter.GreeterClient(invoker);
            string user = "you";
            
            var reply = client.SayHello(new HelloRequest { Name = user });
            Console.WriteLine("Greeting: " + reply.Message);
            
            channel.ShutdownAsync().Wait();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
