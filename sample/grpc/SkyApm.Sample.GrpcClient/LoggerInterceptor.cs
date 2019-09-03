using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Core.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkyApm.Sample.GrpcClient
{
    public class LoggerInterceptor : Interceptor
    {
        private readonly Action _callBack;

        public LoggerInterceptor(Action callBack)
        {
            //this.logger = logger;
            _callBack = callBack;
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            Console.WriteLine($"{Environment.NewLine}GRPC Request{Environment.NewLine}Method: {context.Method}{Environment.NewLine}Data: {JsonConvert.SerializeObject(request, Formatting.Indented)}");
            var response = base.BlockingUnaryCall(request, context, continuation);
            Console.WriteLine($"{Environment.NewLine}GRPC Response{Environment.NewLine}Method: {context.Method}{Environment.NewLine}Data: {JsonConvert.SerializeObject(response, Formatting.Indented)}");
            return response;
        }
    }
}
