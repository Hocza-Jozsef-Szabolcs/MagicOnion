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
            public global::MagicOnion.Client.Internal.RawMethodInvoker<global::MessagePack.Nil, global::TempProject.MyGenericObject<global::System.Int32>> A;
            public global::MagicOnion.Client.Internal.RawMethodInvoker<global::MessagePack.Nil, global::TempProject.MyGenericObject<global::TempProject.MyObject>> B;
            public ClientCore(global::MagicOnion.Serialization.IMagicOnionSerializerProvider serializerProvider)
            {
                this.A = global::MagicOnion.Client.Internal.RawMethodInvoker.Create_ValueType_RefType<global::MessagePack.Nil, global::TempProject.MyGenericObject<global::System.Int32>>(global::Grpc.Core.MethodType.Unary, "IMyService", "A", serializerProvider);
                this.B = global::MagicOnion.Client.Internal.RawMethodInvoker.Create_ValueType_RefType<global::MessagePack.Nil, global::TempProject.MyGenericObject<global::TempProject.MyObject>>(global::Grpc.Core.MethodType.Unary, "IMyService", "B", serializerProvider);
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
        
        public global::MagicOnion.UnaryResult<global::TempProject.MyGenericObject<global::System.Int32>> A()
            => this.core.A.InvokeUnary(this, "IMyService/A", global::MessagePack.Nil.Default);
        public global::MagicOnion.UnaryResult<global::TempProject.MyGenericObject<global::TempProject.MyObject>> B()
            => this.core.B.InvokeUnary(this, "IMyService/B", global::MessagePack.Nil.Default);
    }
}

