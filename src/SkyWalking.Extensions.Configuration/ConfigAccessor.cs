using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using SkyWalking.Config;

// ReSharper disable StringLiteralTypo
namespace SkyWalking.Extensions.Configuration
{
    public class ConfigAccessor : IConfigAccessor
    {
        private const string CONFIG_FILE_PATH = "SKYWALKING__CONFIG__PATH";
        private readonly IConfiguration _configuration;

        public ConfigAccessor(IEnvironmentProvider environmentProvider)
        {
            var builder = new ConfigurationBuilder();

            builder.AddSkyWalkingDefaultConfig();

            builder.AddJsonFile("appsettings.json", true).AddJsonFile($"appsettings.{environmentProvider.EnvironmentName}.json", true);

            builder.AddJsonFile("skywalking.json", true).AddJsonFile($"skywalking.{environmentProvider.EnvironmentName}.json", true);

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(CONFIG_FILE_PATH)))
            {
                builder.AddJsonFile(Environment.GetEnvironmentVariable(CONFIG_FILE_PATH), false);
            }

            builder.AddEnvironmentVariables();

            _configuration = builder.Build();
        }

        public T Get<T>() where T : class, new()
        {
            var config = typeof(T).GetCustomAttribute<ConfigAttribute>();
            var instance = Activator.CreateInstance<T>();
            _configuration.GetSection(config.GetSections()).Bind(instance);
            return instance;
        }

        public T Value<T>(string key, params string[] sections)
        {
            var config = new ConfigAttribute(sections);
            return _configuration.GetSection(config.GetSections()).GetValue<T>(key);
        }
    }
}