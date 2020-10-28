// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: protos/GStoreClient.proto
// </auto-generated>
#pragma warning disable 0414, 1591
#region Designer generated code

using grpc = global::Grpc.Core;

public static partial class GStoreClientService
{
  static readonly string __ServiceName = "GStoreClientService";

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

  static class __Helper_MessageCache<T>
  {
    public static readonly bool IsBufferMessage = global::System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(global::Google.Protobuf.IBufferMessage)).IsAssignableFrom(typeof(T));
  }

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

  static readonly grpc::Marshaller<global::RecvMsgRequest> __Marshaller_RecvMsgRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::RecvMsgRequest.Parser));
  static readonly grpc::Marshaller<global::RecvMsgReply> __Marshaller_RecvMsgReply = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::RecvMsgReply.Parser));
  static readonly grpc::Marshaller<global::ReadValueRequest> __Marshaller_ReadValueRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::ReadValueRequest.Parser));
  static readonly grpc::Marshaller<global::ReadValueReply> __Marshaller_ReadValueReply = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::ReadValueReply.Parser));
  static readonly grpc::Marshaller<global::WriteValueRequest> __Marshaller_WriteValueRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::WriteValueRequest.Parser));
  static readonly grpc::Marshaller<global::WriteValueReply> __Marshaller_WriteValueReply = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::WriteValueReply.Parser));
  static readonly grpc::Marshaller<global::ListServerObjectsRequest> __Marshaller_ListServerObjectsRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::ListServerObjectsRequest.Parser));
  static readonly grpc::Marshaller<global::ListServerObjectsReply> __Marshaller_ListServerObjectsReply = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::ListServerObjectsReply.Parser));
  static readonly grpc::Marshaller<global::ListGlobalRequest> __Marshaller_ListGlobalRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::ListGlobalRequest.Parser));
  static readonly grpc::Marshaller<global::ListGlobalReply> __Marshaller_ListGlobalReply = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::ListGlobalReply.Parser));

  static readonly grpc::Method<global::RecvMsgRequest, global::RecvMsgReply> __Method_RecvMsg = new grpc::Method<global::RecvMsgRequest, global::RecvMsgReply>(
      grpc::MethodType.Unary,
      __ServiceName,
      "RecvMsg",
      __Marshaller_RecvMsgRequest,
      __Marshaller_RecvMsgReply);

  static readonly grpc::Method<global::ReadValueRequest, global::ReadValueReply> __Method_ReadValue = new grpc::Method<global::ReadValueRequest, global::ReadValueReply>(
      grpc::MethodType.Unary,
      __ServiceName,
      "ReadValue",
      __Marshaller_ReadValueRequest,
      __Marshaller_ReadValueReply);

  static readonly grpc::Method<global::WriteValueRequest, global::WriteValueReply> __Method_WriteValue = new grpc::Method<global::WriteValueRequest, global::WriteValueReply>(
      grpc::MethodType.Unary,
      __ServiceName,
      "WriteValue",
      __Marshaller_WriteValueRequest,
      __Marshaller_WriteValueReply);

  static readonly grpc::Method<global::ListServerObjectsRequest, global::ListServerObjectsReply> __Method_ListServerObjects = new grpc::Method<global::ListServerObjectsRequest, global::ListServerObjectsReply>(
      grpc::MethodType.Unary,
      __ServiceName,
      "ListServerObjects",
      __Marshaller_ListServerObjectsRequest,
      __Marshaller_ListServerObjectsReply);

  static readonly grpc::Method<global::ListGlobalRequest, global::ListGlobalReply> __Method_ListGlobal = new grpc::Method<global::ListGlobalRequest, global::ListGlobalReply>(
      grpc::MethodType.Unary,
      __ServiceName,
      "ListGlobal",
      __Marshaller_ListGlobalRequest,
      __Marshaller_ListGlobalReply);

  /// <summary>Service descriptor</summary>
  public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
  {
    get { return global::GStoreClientReflection.Descriptor.Services[0]; }
  }

  /// <summary>Base class for server-side implementations of GStoreClientService</summary>
  [grpc::BindServiceMethod(typeof(GStoreClientService), "BindService")]
  public abstract partial class GStoreClientServiceBase
  {
    public virtual global::System.Threading.Tasks.Task<global::RecvMsgReply> RecvMsg(global::RecvMsgRequest request, grpc::ServerCallContext context)
    {
      throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
    }

    public virtual global::System.Threading.Tasks.Task<global::ReadValueReply> ReadValue(global::ReadValueRequest request, grpc::ServerCallContext context)
    {
      throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
    }

    public virtual global::System.Threading.Tasks.Task<global::WriteValueReply> WriteValue(global::WriteValueRequest request, grpc::ServerCallContext context)
    {
      throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
    }

    public virtual global::System.Threading.Tasks.Task<global::ListServerObjectsReply> ListServerObjects(global::ListServerObjectsRequest request, grpc::ServerCallContext context)
    {
      throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
    }

    public virtual global::System.Threading.Tasks.Task<global::ListGlobalReply> ListGlobal(global::ListGlobalRequest request, grpc::ServerCallContext context)
    {
      throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
    }

  }

  /// <summary>Creates service definition that can be registered with a server</summary>
  /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
  public static grpc::ServerServiceDefinition BindService(GStoreClientServiceBase serviceImpl)
  {
    return grpc::ServerServiceDefinition.CreateBuilder()
        .AddMethod(__Method_RecvMsg, serviceImpl.RecvMsg)
        .AddMethod(__Method_ReadValue, serviceImpl.ReadValue)
        .AddMethod(__Method_WriteValue, serviceImpl.WriteValue)
        .AddMethod(__Method_ListServerObjects, serviceImpl.ListServerObjects)
        .AddMethod(__Method_ListGlobal, serviceImpl.ListGlobal).Build();
  }

  /// <summary>Register service method with a service binder with or without implementation. Useful when customizing the  service binding logic.
  /// Note: this method is part of an experimental API that can change or be removed without any prior notice.</summary>
  /// <param name="serviceBinder">Service methods will be bound by calling <c>AddMethod</c> on this object.</param>
  /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
  public static void BindService(grpc::ServiceBinderBase serviceBinder, GStoreClientServiceBase serviceImpl)
  {
    serviceBinder.AddMethod(__Method_RecvMsg, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::RecvMsgRequest, global::RecvMsgReply>(serviceImpl.RecvMsg));
    serviceBinder.AddMethod(__Method_ReadValue, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::ReadValueRequest, global::ReadValueReply>(serviceImpl.ReadValue));
    serviceBinder.AddMethod(__Method_WriteValue, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::WriteValueRequest, global::WriteValueReply>(serviceImpl.WriteValue));
    serviceBinder.AddMethod(__Method_ListServerObjects, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::ListServerObjectsRequest, global::ListServerObjectsReply>(serviceImpl.ListServerObjects));
    serviceBinder.AddMethod(__Method_ListGlobal, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::ListGlobalRequest, global::ListGlobalReply>(serviceImpl.ListGlobal));
  }

}
#endregion