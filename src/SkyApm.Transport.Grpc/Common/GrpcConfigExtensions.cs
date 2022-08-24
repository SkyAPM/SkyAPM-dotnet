using Grpc.Core;
using SkyApm.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyApm.Transport.Grpc.Common
{
    public static class GrpcConfigExtensions
    {
        public static Metadata GetMeta(this GrpcConfig config)
        {
            if (string.IsNullOrEmpty(config.Authentication))
            {
                return null;
            }
            return new Metadata { new Metadata.Entry("Authentication", config.Authentication) };
        }
    }
}
