// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: JVMMetricsService.proto
// </auto-generated>
#pragma warning disable 1591
#region Designer generated code

using System;
using System.Threading;
using System.Threading.Tasks;
using grpc = global::Grpc.Core;

namespace SkyWalking.NetworkProtocol {
  public static partial class JVMMetricsService
  {
    static readonly string __ServiceName = "JVMMetricsService";

    static readonly grpc::Marshaller<global::SkyWalking.NetworkProtocol.JVMMetrics> __Marshaller_JVMMetrics = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::SkyWalking.NetworkProtocol.JVMMetrics.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::SkyWalking.NetworkProtocol.Downstream> __Marshaller_Downstream = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::SkyWalking.NetworkProtocol.Downstream.Parser.ParseFrom);

    static readonly grpc::Method<global::SkyWalking.NetworkProtocol.JVMMetrics, global::SkyWalking.NetworkProtocol.Downstream> __Method_collect = new grpc::Method<global::SkyWalking.NetworkProtocol.JVMMetrics, global::SkyWalking.NetworkProtocol.Downstream>(
        grpc::MethodType.Unary,
        __ServiceName,
        "collect",
        __Marshaller_JVMMetrics,
        __Marshaller_Downstream);

    /// <summary>Service descriptor</summary>
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::SkyWalking.NetworkProtocol.JVMMetricsServiceReflection.Descriptor.Services[0]; }
    }

    /// <summary>Base class for server-side implementations of JVMMetricsService</summary>
    public abstract partial class JVMMetricsServiceBase
    {
      public virtual global::System.Threading.Tasks.Task<global::SkyWalking.NetworkProtocol.Downstream> collect(global::SkyWalking.NetworkProtocol.JVMMetrics request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

    }

    /// <summary>Client for JVMMetricsService</summary>
    public partial class JVMMetricsServiceClient : grpc::ClientBase<JVMMetricsServiceClient>
    {
      /// <summary>Creates a new client for JVMMetricsService</summary>
      /// <param name="channel">The channel to use to make remote calls.</param>
      public JVMMetricsServiceClient(grpc::Channel channel) : base(channel)
      {
      }
      /// <summary>Creates a new client for JVMMetricsService that uses a custom <c>CallInvoker</c>.</summary>
      /// <param name="callInvoker">The callInvoker to use to make remote calls.</param>
      public JVMMetricsServiceClient(grpc::CallInvoker callInvoker) : base(callInvoker)
      {
      }
      /// <summary>Protected parameterless constructor to allow creation of test doubles.</summary>
      protected JVMMetricsServiceClient() : base()
      {
      }
      /// <summary>Protected constructor to allow creation of configured clients.</summary>
      /// <param name="configuration">The client configuration.</param>
      protected JVMMetricsServiceClient(ClientBaseConfiguration configuration) : base(configuration)
      {
      }

      public virtual global::SkyWalking.NetworkProtocol.Downstream collect(global::SkyWalking.NetworkProtocol.JVMMetrics request, grpc::Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
      {
        return collect(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::SkyWalking.NetworkProtocol.Downstream collect(global::SkyWalking.NetworkProtocol.JVMMetrics request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_collect, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::SkyWalking.NetworkProtocol.Downstream> collectAsync(global::SkyWalking.NetworkProtocol.JVMMetrics request, grpc::Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
      {
        return collectAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::SkyWalking.NetworkProtocol.Downstream> collectAsync(global::SkyWalking.NetworkProtocol.JVMMetrics request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_collect, null, options, request);
      }
      /// <summary>Creates a new instance of client from given <c>ClientBaseConfiguration</c>.</summary>
      protected override JVMMetricsServiceClient NewInstance(ClientBaseConfiguration configuration)
      {
        return new JVMMetricsServiceClient(configuration);
      }
    }

    /// <summary>Creates service definition that can be registered with a server</summary>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    public static grpc::ServerServiceDefinition BindService(JVMMetricsServiceBase serviceImpl)
    {
      return grpc::ServerServiceDefinition.CreateBuilder()
          .AddMethod(__Method_collect, serviceImpl.collect).Build();
    }

  }
}
#endregion
