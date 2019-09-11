using Grpc.Core;
using Grpc.Core.Interceptors;
using GrpcGreeter;
using SkyApm.Diagnostics.Grpc.Server;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace SkyApm.Sample.GrpcServer
{
    public static class Extensions
    {
        public static IServiceProvider StartGrpcServer(this IServiceProvider provider)
        {
            var interceptor = provider.GetService<ServerDiagnosticInterceptor>();
            var definition = Greeter.BindService(new GreeterImpl());
            if (interceptor != null)
            {
                definition = definition.Intercept(interceptor);
            }
            int port = 12345;
            Server server = new Server
            {
                Services = { definition },
                Ports = { new ServerPort("localhost", port, ServerCredentials.Insecure) },
            };
            server.Start();

            Console.WriteLine("Greeter server listening on port " + port);
            return provider;
        }
    }
}
