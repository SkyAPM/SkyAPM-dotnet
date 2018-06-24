using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using SkyWalking.Remote.Authentication;

namespace SkyWalking.Remote
{
    internal class GrpcChannelBuilder
    {
        private string _token;

        private string _server;

        private string _rootCertificatePath;

        public GrpcChannelBuilder WithAuthenticationToken(string token)
        {
            _token = token;
            return this;
        }

        public GrpcChannelBuilder WithServer(string server)
        {
            _server = server;
            return this;
        }

        public GrpcChannelBuilder WithCredential(string rootCertificatePath)
        {
            _rootCertificatePath = rootCertificatePath;
            return this;
        }

        public Channel Build()
        {
            return new Channel(_server, GetCredentials());
        }

        private ChannelCredentials GetCredentials()
        {
            if (_rootCertificatePath != null)
            {
                var authInterceptor = AuthenticationInterceptors.CreateAuthInterceptor(_token);
                return ChannelCredentials.Create(new SslCredentials(), CallCredentials.FromInterceptor(authInterceptor));
            }
            return ChannelCredentials.Insecure;
        }
    }
}
