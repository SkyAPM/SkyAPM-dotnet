using Grpc.Core;
using Grpc.Core.Interceptors;
using System;

namespace SkyApm.Diagnostics.Grpc.Client
{
    public interface IClientDiagnosticProcessor
    {
        Metadata BeginRequest<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> grpcContext) where TRequest : class where TResponse : class;

        void EndRequest();

        void DiagnosticUnhandledException(Exception exception);
    }
}
