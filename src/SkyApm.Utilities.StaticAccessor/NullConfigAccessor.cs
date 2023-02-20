using SkyApm.Config;

namespace SkyApm.Utilities.StaticAccessor
{
    internal class NullConfigAccessor : IConfigAccessor
    {
        public T Get<T>() where T : class, new() => new T();

        public T Value<T>(string key, params string[] sections) => default;
    }
}
