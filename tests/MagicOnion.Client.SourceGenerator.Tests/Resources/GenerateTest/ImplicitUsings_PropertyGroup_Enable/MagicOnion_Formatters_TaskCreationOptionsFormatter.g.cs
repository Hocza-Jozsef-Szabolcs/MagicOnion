﻿// <auto-generated />
#pragma warning disable CS0618 // 'member' is obsolete: 'text'
#pragma warning disable CS0612 // 'member' is obsolete
#pragma warning disable CS8019 // Unnecessary using directive.

namespace MagicOnion.Formatters
{
    using global::System;
    using global::MessagePack;

    public sealed class TaskCreationOptionsFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::System.Threading.Tasks.TaskCreationOptions>
    {
        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::System.Threading.Tasks.TaskCreationOptions value, global::MessagePack.MessagePackSerializerOptions options)
        {
            writer.Write((Int32)value);
        }
        
        public global::System.Threading.Tasks.TaskCreationOptions Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            return (global::System.Threading.Tasks.TaskCreationOptions)reader.ReadInt32();
        }
    }
}
