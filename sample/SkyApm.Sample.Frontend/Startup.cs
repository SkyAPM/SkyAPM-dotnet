using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SkyApm.Sample.Backend.Services;

#if NETCOREAPP2_1

using IHostEnvironment = Microsoft.Extensions.Hosting.IHostingEnvironment;

#endif

namespace SkyApm.Sample.Frontend
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
#if NETCOREAPP2_1

            services.AddMvc();

#else
             services.AddControllers();
#endif

            // DI grpc service
            services.AddSingleton<GreeterGrpcService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
#if NETCOREAPP2_1
            app.UseMvcWithDefaultRoute();
#else
            app.UseRouting();

            app.UseEndpoints(endpoint =>
            {
                endpoint.MapDefaultControllerRoute();
            });
#endif
        }
    }
}