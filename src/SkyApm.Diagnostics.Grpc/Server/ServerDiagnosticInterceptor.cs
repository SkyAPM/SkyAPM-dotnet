using Grpc.Core;
using Grpc.Core.Interceptors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkyApm.Diagnostics.Grpc.Server
{
    public class ServerDiagnosticInterceptor : Interceptor
    {
        private readonly ServerDiagnosticProcessor _processor;
        public ServerDiagnosticInterceptor(ServerDiagnosticProcessor processor)
        {
            _processor = processor;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            _processor.BeginRequest(context);
            try
            {
                var response = await continuation(request, context);
                _processor.EndRequest(context);
                return response;
            }
            catch (Exception ex)
            {
                _processor.DiagnosticUnhandledException(ex);
                throw ex;
            }
        }
    }
}
