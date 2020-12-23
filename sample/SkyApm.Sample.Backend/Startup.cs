using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SkyApm.Sample.Backend.Models;
using SkyApm.Sample.Backend.Sampling;
using SkyApm.Sample.Backend.Services;
using SkyApm.Sample.GrpcServer;
using SkyApm.Tracing;

#if NETCOREAPP2_1

using IHostEnvironment = Microsoft.Extensions.Hosting.IHostingEnvironment;

#endif

namespace SkyApm.Sample.Backend
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

            var sqlLiteConnection = new SqliteConnection("DataSource=:memory:");
            sqlLiteConnection.Open();

            services.AddEntityFrameworkSqlite().AddDbContext<SampleDbContext>(c => c.UseSqlite(sqlLiteConnection));

            services.AddSingleton<ISamplingInterceptor, CustomSamplingInterceptor>();

            // DI grpc service
            services.AddSingleton<GreeterGrpcService>();

#if !NETCOREAPP2_1

            services.AddGrpc();
#endif
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            using (var scope = app.ApplicationServices.CreateScope())
            using (var sampleDbContext = scope.ServiceProvider.GetService<SampleDbContext>())
            {
                sampleDbContext.Database.EnsureCreated();
            }

#if NETCOREAPP2_1
            app.UseMvcWithDefaultRoute();
#else
            app.UseRouting();

            app.UseEndpoints(endpoint =>
            {
                endpoint.MapDefaultControllerRoute();
                endpoint.MapGrpcService<GreeterImpl>();
            });
#endif
        }
    }
}