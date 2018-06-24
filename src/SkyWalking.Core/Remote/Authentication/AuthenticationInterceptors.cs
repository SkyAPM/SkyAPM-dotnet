using Grpc.Core;
using SkyWalking.Utils;

namespace SkyWalking.Remote.Authentication
{
    internal class AuthenticationInterceptors
    {
        private const string header = "authentication";
        
        public static AsyncAuthInterceptor CreateAuthInterceptor(string token)
        {
            return (context, metadata) =>
            {
                var entry = CreateTokenHeader(token);
                if (entry != null)
                {
                    metadata.Add(entry);
                }    
                return TaskUtils.CompletedTask;
            };
        }

        private static Metadata.Entry CreateTokenHeader(string token)
        {
            return string.IsNullOrEmpty(token) ? null : new Metadata.Entry(header, token);
        }
    }
}
