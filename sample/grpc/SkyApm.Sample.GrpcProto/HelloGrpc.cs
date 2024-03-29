// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: hello.proto
// </auto-generated>
#pragma warning disable 0414, 1591
#region Designer generated code

using grpc = global::Grpc.Core;

namespace GrpcGreeter {
  public static partial class Greeter
  {
    static readonly string __ServiceName = "GrpcGreeter.Greeter";

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static void __Helper_SerializeMessage(global::Google.Protobuf.IMessage message, grpc::SerializationContext context)
    {
      #if !GRPC_DISABLE_PROTOBUF_BUFFER_SERIALIZATION
      if (message is global::Google.Protobuf.IBufferMessage)
      {
        context.SetPayloadLength(message.CalculateSize());
        global::Google.Protobuf.MessageExtensions.WriteTo(message, context.GetBufferWriter());
        context.Complete();
        return;
      }
      #endif
      context.Complete(global::Google.Protobuf.MessageExtensions.ToByteArray(message));
    }

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static class __Helper_MessageCache<T>
    {
      public static readonly bool IsBufferMessage = global::System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(global::Google.Protobuf.IBufferMessage)).IsAssignableFrom(typeof(T));
    }

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static T __Helper_DeserializeMessage<T>(grpc::DeserializationContext context, global::Google.Protobuf.MessageParser<T> parser) where T : global::Google.Protobuf.IMessage<T>
    {
      #if !GRPC_DISABLE_PROTOBUF_BUFFER_SERIALIZATION
      if (__Helper_MessageCache<T>.IsBufferMessage)
      {
        return parser.ParseFrom(context.PayloadAsReadOnlySequence());
      }
      #endif
      return parser.ParseFrom(context.PayloadAsNewBuffer());
    }

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::GrpcGreeter.HelloRequest> __Marshaller_GrpcGreeter_HelloRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::GrpcGreeter.HelloRequest.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::GrpcGreeter.HelloReply> __Marshaller_GrpcGreeter_HelloReply = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::GrpcGreeter.HelloReply.Parser));

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::GrpcGreeter.HelloRequest, global::GrpcGreeter.HelloReply> __Method_SayHello = new grpc::Method<global::GrpcGreeter.HelloRequest, global::GrpcGreeter.HelloReply>(
        grpc::MethodType.Unary,
        __ServiceName,
        "SayHello",
        __Marshaller_GrpcGreeter_HelloRequest,
        __Marshaller_GrpcGreeter_HelloReply);

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::GrpcGreeter.HelloRequest, global::GrpcGreeter.HelloReply> __Method_SayHelloWithException = new grpc::Method<global::GrpcGreeter.HelloRequest, global::GrpcGreeter.HelloReply>(
        grpc::MethodType.Unary,
        __ServiceName,
        "SayHelloWithException",
        __Marshaller_GrpcGreeter_HelloRequest,
        __Marshaller_GrpcGreeter_HelloReply);

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::GrpcGreeter.HelloRequest, global::GrpcGreeter.HelloReply> __Method_SayHelloByServerStreaming = new grpc::Method<global::GrpcGreeter.HelloRequest, global::GrpcGreeter.HelloReply>(
        grpc::MethodType.ServerStreaming,
        __ServiceName,
        "SayHelloByServerStreaming",
        __Marshaller_GrpcGreeter_HelloRequest,
        __Marshaller_GrpcGreeter_HelloReply);

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::GrpcGreeter.HelloRequest, global::GrpcGreeter.HelloReply> __Method_SayHelloByClientStreaming = new grpc::Method<global::GrpcGreeter.HelloRequest, global::GrpcGreeter.HelloReply>(
        grpc::MethodType.ClientStreaming,
        __ServiceName,
        "SayHelloByClientStreaming",
        __Marshaller_GrpcGreeter_HelloRequest,
        __Marshaller_GrpcGreeter_HelloReply);

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::GrpcGreeter.HelloRequest, global::GrpcGreeter.HelloReply> __Method_SayHelloByDuplexStreaming = new grpc::Method<global::GrpcGreeter.HelloRequest, global::GrpcGreeter.HelloReply>(
        grpc::MethodType.DuplexStreaming,
        __ServiceName,
        "SayHelloByDuplexStreaming",
        __Marshaller_GrpcGreeter_HelloRequest,
        __Marshaller_GrpcGreeter_HelloReply);

    /// <summary>Service descriptor</summary>
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::GrpcGreeter.HelloReflection.Descriptor.Services[0]; }
    }

    /// <summary>Base class for server-side implementations of Greeter</summary>
    [grpc::BindServiceMethod(typeof(Greeter), "BindService")]
    public abstract partial class GreeterBase
    {
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::System.Threading.Tasks.Task<global::GrpcGreeter.HelloReply> SayHello(global::GrpcGreeter.HelloRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::System.Threading.Tasks.Task<global::GrpcGreeter.HelloReply> SayHelloWithException(global::GrpcGreeter.HelloRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::System.Threading.Tasks.Task SayHelloByServerStreaming(global::GrpcGreeter.HelloRequest request, grpc::IServerStreamWriter<global::GrpcGreeter.HelloReply> responseStream, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::System.Threading.Tasks.Task<global::GrpcGreeter.HelloReply> SayHelloByClientStreaming(grpc::IAsyncStreamReader<global::GrpcGreeter.HelloRequest> requestStream, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::System.Threading.Tasks.Task SayHelloByDuplexStreaming(grpc::IAsyncStreamReader<global::GrpcGreeter.HelloRequest> requestStream, grpc::IServerStreamWriter<global::GrpcGreeter.HelloReply> responseStream, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

    }

    /// <summary>Client for Greeter</summary>
    public partial class GreeterClient : grpc::ClientBase<GreeterClient>
    {
      /// <summary>Creates a new client for Greeter</summary>
      /// <param name="channel">The channel to use to make remote calls.</param>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public GreeterClient(grpc::ChannelBase channel) : base(channel)
      {
      }
      /// <summary>Creates a new client for Greeter that uses a custom <c>CallInvoker</c>.</summary>
      /// <param name="callInvoker">The callInvoker to use to make remote calls.</param>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public GreeterClient(grpc::CallInvoker callInvoker) : base(callInvoker)
      {
      }
      /// <summary>Protected parameterless constructor to allow creation of test doubles.</summary>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      protected GreeterClient() : base()
      {
      }
      /// <summary>Protected constructor to allow creation of configured clients.</summary>
      /// <param name="configuration">The client configuration.</param>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      protected GreeterClient(ClientBaseConfiguration configuration) : base(configuration)
      {
      }

      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::GrpcGreeter.HelloReply SayHello(global::GrpcGreeter.HelloRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return SayHello(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::GrpcGreeter.HelloReply SayHello(global::GrpcGreeter.HelloRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_SayHello, null, options, request);
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::GrpcGreeter.HelloReply> SayHelloAsync(global::GrpcGreeter.HelloRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return SayHelloAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::GrpcGreeter.HelloReply> SayHelloAsync(global::GrpcGreeter.HelloRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_SayHello, null, options, request);
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::GrpcGreeter.HelloReply SayHelloWithException(global::GrpcGreeter.HelloRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return SayHelloWithException(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::GrpcGreeter.HelloReply SayHelloWithException(global::GrpcGreeter.HelloRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_SayHelloWithException, null, options, request);
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::GrpcGreeter.HelloReply> SayHelloWithExceptionAsync(global::GrpcGreeter.HelloRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return SayHelloWithExceptionAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::GrpcGreeter.HelloReply> SayHelloWithExceptionAsync(global::GrpcGreeter.HelloRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_SayHelloWithException, null, options, request);
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncServerStreamingCall<global::GrpcGreeter.HelloReply> SayHelloByServerStreaming(global::GrpcGreeter.HelloRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return SayHelloByServerStreaming(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncServerStreamingCall<global::GrpcGreeter.HelloReply> SayHelloByServerStreaming(global::GrpcGreeter.HelloRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncServerStreamingCall(__Method_SayHelloByServerStreaming, null, options, request);
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncClientStreamingCall<global::GrpcGreeter.HelloRequest, global::GrpcGreeter.HelloReply> SayHelloByClientStreaming(grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return SayHelloByClientStreaming(new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncClientStreamingCall<global::GrpcGreeter.HelloRequest, global::GrpcGreeter.HelloReply> SayHelloByClientStreaming(grpc::CallOptions options)
      {
        return CallInvoker.AsyncClientStreamingCall(__Method_SayHelloByClientStreaming, null, options);
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncDuplexStreamingCall<global::GrpcGreeter.HelloRequest, global::GrpcGreeter.HelloReply> SayHelloByDuplexStreaming(grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return SayHelloByDuplexStreaming(new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncDuplexStreamingCall<global::GrpcGreeter.HelloRequest, global::GrpcGreeter.HelloReply> SayHelloByDuplexStreaming(grpc::CallOptions options)
      {
        return CallInvoker.AsyncDuplexStreamingCall(__Method_SayHelloByDuplexStreaming, null, options);
      }
      /// <summary>Creates a new instance of client from given <c>ClientBaseConfiguration</c>.</summary>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      protected override GreeterClient NewInstance(ClientBaseConfiguration configuration)
      {
        return new GreeterClient(configuration);
      }
    }

    /// <summary>Creates service definition that can be registered with a server</summary>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    public static grpc::ServerServiceDefinition BindService(GreeterBase serviceImpl)
    {
      return grpc::ServerServiceDefinition.CreateBuilder()
          .AddMethod(__Method_SayHello, serviceImpl.SayHello)
          .AddMethod(__Method_SayHelloWithException, serviceImpl.SayHelloWithException)
          .AddMethod(__Method_SayHelloByServerStreaming, serviceImpl.SayHelloByServerStreaming)
          .AddMethod(__Method_SayHelloByClientStreaming, serviceImpl.SayHelloByClientStreaming)
          .AddMethod(__Method_SayHelloByDuplexStreaming, serviceImpl.SayHelloByDuplexStreaming).Build();
    }

    /// <summary>Register service method with a service binder with or without implementation. Useful when customizing the  service binding logic.
    /// Note: this method is part of an experimental API that can change or be removed without any prior notice.</summary>
    /// <param name="serviceBinder">Service methods will be bound by calling <c>AddMethod</c> on this object.</param>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    public static void BindService(grpc::ServiceBinderBase serviceBinder, GreeterBase serviceImpl)
    {
      serviceBinder.AddMethod(__Method_SayHello, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::GrpcGreeter.HelloRequest, global::GrpcGreeter.HelloReply>(serviceImpl.SayHello));
      serviceBinder.AddMethod(__Method_SayHelloWithException, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::GrpcGreeter.HelloRequest, global::GrpcGreeter.HelloReply>(serviceImpl.SayHelloWithException));
      serviceBinder.AddMethod(__Method_SayHelloByServerStreaming, serviceImpl == null ? null : new grpc::ServerStreamingServerMethod<global::GrpcGreeter.HelloRequest, global::GrpcGreeter.HelloReply>(serviceImpl.SayHelloByServerStreaming));
      serviceBinder.AddMethod(__Method_SayHelloByClientStreaming, serviceImpl == null ? null : new grpc::ClientStreamingServerMethod<global::GrpcGreeter.HelloRequest, global::GrpcGreeter.HelloReply>(serviceImpl.SayHelloByClientStreaming));
      serviceBinder.AddMethod(__Method_SayHelloByDuplexStreaming, serviceImpl == null ? null : new grpc::DuplexStreamingServerMethod<global::GrpcGreeter.HelloRequest, global::GrpcGreeter.HelloReply>(serviceImpl.SayHelloByDuplexStreaming));
    }

  }
}
#endregion
