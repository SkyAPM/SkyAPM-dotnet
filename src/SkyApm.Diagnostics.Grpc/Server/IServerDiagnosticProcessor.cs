using Grpc.Core;
using System;

namespace SkyApm.Diagnostics.Grpc.Server
{
    public interface IServerDiagnosticProcessor
    {
        void BeginRequest(ServerCallContext grpcContext);

        void EndRequest(ServerCallContext grpcContext);

        void DiagnosticUnhandledException(Exception exception);
    }
}
