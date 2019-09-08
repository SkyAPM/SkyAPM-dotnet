using Grpc.Core;
using Grpc.Core.Interceptors;
using GrpcGreeter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SkyApm.Diagnostics.Grpc.Server;
using System;

namespace SkyApm.Sample.GrpcServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            return services.BuildServiceProvider();
        }

        public void Use(IServiceProvider provider)
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
            //Console.WriteLine("Press any key to stop the server...");
            //Console.ReadKey();
            //server.ShutdownAsync().Wait();
        }
    }
}
