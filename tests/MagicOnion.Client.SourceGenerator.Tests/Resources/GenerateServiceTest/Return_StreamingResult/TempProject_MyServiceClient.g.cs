﻿// <auto-generated />
#pragma warning disable CS0618 // 'member' is obsolete: 'text'
#pragma warning disable CS0612 // 'member' is obsolete
#pragma warning disable CS8019 // Unnecessary using directive.


namespace TempProject
{
    using global::System;
    using global::Grpc.Core;
    using global::MagicOnion;
    using global::MagicOnion.Client;
    using global::MessagePack;
    
    [global::MagicOnion.Ignore]
    public class MyServiceClient : global::MagicOnion.Client.MagicOnionClientBase<global::TempProject.IMyService>, global::TempProject.IMyService
    {
        class ClientCore
        {
            public global::MagicOnion.Client.Internal.RawMethodInvoker<global::System.String, global::System.String> ClientStreamingAsync;
            public global::MagicOnion.Client.Internal.RawMethodInvoker<global::MessagePack.Nil, global::System.String> ServerStreamingAsync;
            public global::MagicOnion.Client.Internal.RawMethodInvoker<global::System.String, global::System.String> DuplexStreamingAsync;
            public ClientCore(global::MagicOnion.Serialization.IMagicOnionSerializerProvider serializerProvider)
            {
                this.ClientStreamingAsync = global::MagicOnion.Client.Internal.RawMethodInvoker.Create_RefType_RefType<global::System.String, global::System.String>(global::Grpc.Core.MethodType.ClientStreaming, "IMyService", "ClientStreamingAsync", serializerProvider);
                this.ServerStreamingAsync = global::MagicOnion.Client.Internal.RawMethodInvoker.Create_ValueType_RefType<global::MessagePack.Nil, global::System.String>(global::Grpc.Core.MethodType.ServerStreaming, "IMyService", "ServerStreamingAsync", serializerProvider);
                this.DuplexStreamingAsync = global::MagicOnion.Client.Internal.RawMethodInvoker.Create_RefType_RefType<global::System.String, global::System.String>(global::Grpc.Core.MethodType.DuplexStreaming, "IMyService", "DuplexStreamingAsync", serializerProvider);
            }
        }
        
        readonly ClientCore core;
        
        public MyServiceClient(global::MagicOnion.Client.MagicOnionClientOptions options, global::MagicOnion.Serialization.IMagicOnionSerializerProvider serializerProvider) : base(options)
        {
            this.core = new ClientCore(serializerProvider);
        }
        
        private MyServiceClient(MagicOnionClientOptions options, ClientCore core) : base(options)
        {
            this.core = core;
        }
        
        protected override global::MagicOnion.Client.MagicOnionClientBase<IMyService> Clone(global::MagicOnion.Client.MagicOnionClientOptions options)
            => new MyServiceClient(options, core);
        
        public global::System.Threading.Tasks.Task<global::MagicOnion.ClientStreamingResult<global::System.String, global::System.String>> ClientStreamingAsync()
            => this.core.ClientStreamingAsync.InvokeClientStreaming(this, "IMyService/ClientStreamingAsync");
        public global::System.Threading.Tasks.Task<global::MagicOnion.ServerStreamingResult<global::System.String>> ServerStreamingAsync()
            => this.core.ServerStreamingAsync.InvokeServerStreaming(this, "IMyService/ServerStreamingAsync", global::MessagePack.Nil.Default);
        public global::System.Threading.Tasks.Task<global::MagicOnion.DuplexStreamingResult<global::System.String, global::System.String>> DuplexStreamingAsync()
            => this.core.DuplexStreamingAsync.InvokeDuplexStreaming(this, "IMyService/DuplexStreamingAsync");
    }
}

