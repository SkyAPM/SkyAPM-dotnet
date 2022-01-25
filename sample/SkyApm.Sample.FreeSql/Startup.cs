using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FreeSql;
using SkyApm.Diagnostics.FreeSql;
using SkyApm.Sample.FreeSqlSqlite.Entitys;
using System;
using System.Collections.Generic;
using SkyApm.Utilities.DependencyInjection;

namespace SkyApm.Sample.FreeSqlSqlite
{
    public class Startup
    {
        private readonly string[] Titles = new string[] { "GetAway", "Candy", "DreamHeaven"};
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            Fsql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.Sqlite, @"DataSource=:memory:;Pooling=true;Max Pool Size=10")
                .UseAutoSyncStructure(true)
                .Build();
            InitData();
            Fsql.Aop.CurdAfter += (s, e) =>
            {
                if (e.ElapsedMilliseconds > 200)
                {
                    //记录日志
                    //发送短信给负责人
                }
            };


        }

        public IConfiguration Configuration { get; }
        public IFreeSql Fsql { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IFreeSql>(Fsql);
            services.AddControllers();
            services.AddSkyApmExtensions();
            services.AddSkyAPM((e) =>
            {
                e.AddFreeSql(Fsql);
            });
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {

            app.UseHttpMethodOverride(new HttpMethodOverrideOptions { FormFieldName = "X-Http-Method-Override" });
            app.UseDeveloperExceptionPage();
            app.UseRouting();
            app.UseEndpoints(a => a.MapControllers());
        }


        public void InitData()
        {
            const int count= 10000;
            var rand = new Random();
            var songs = new List<Song>(count);
            for (int i = 0; i < count; i++)
            {
                songs.Add(new Song() { Id = i + 1, Title = Titles[rand.Next(Titles.Length - 1)] });

            }
            Fsql.Insert<Song>(songs).ExecuteIdentity();


        }
    }

}