﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version: 17.0.0.0
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace MagicOnion.Client.SourceGenerator.CodeGen
{
    using System.Linq;
    using System.Text;
    using System.Collections.Generic;
    using MagicOnion.Client.SourceGenerator.CodeGen.Extensions;
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "17.0.0.0")]
    public partial class RegisterTemplate : RegisterTemplateBase
    {
        /// <summary>
        /// Create the template output
        /// </summary>
        public virtual string TransformText()
        {
            this.Write(@"#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 219
#pragma warning disable 168

// NOTE: Disable warnings for nullable reference types.
// `#nullable disable` causes compile error on old C# compilers (-7.3)
#pragma warning disable 8603 // Possible null reference return.
#pragma warning disable 8618 // Non-nullable variable must contain a non-null value when exiting constructor. Consider declaring it as nullable.
#pragma warning disable 8625 // Cannot convert null literal to non-nullable reference type.

namespace ");
            this.Write(this.ToStringHelper.ToStringWithCulture(Namespace));
            this.Write(@"
{
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Linq;
    using global::MagicOnion;
    using global::MagicOnion.Client;

    public static partial class MagicOnionInitializer
    {
        static bool isRegistered = false;

");
 if (!DisableAutoRegisterOnInitialize) { 
            this.Write("#if UNITY_2019_4_OR_NEWER\r\n        [UnityEngine.RuntimeInitializeOnLoadMethod(Uni" +
                    "tyEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]\r\n#elif NET5_0_OR_GREATER\r\n " +
                    "       [System.Runtime.CompilerServices.ModuleInitializer]\r\n#endif\r\n");
 } 
            this.Write("        public static void Register()\r\n        {\r\n            if (isRegistered) r" +
                    "eturn;\r\n            isRegistered = true;\r\n\r\n            global::MagicOnion.Clien" +
                    "t.MagicOnionClientFactoryProvider.Default =\r\n                (global::MagicOnion" +
                    ".Client.MagicOnionClientFactoryProvider.Default is global::MagicOnion.Client.Imm" +
                    "utableMagicOnionClientFactoryProvider immutableMagicOnionClientFactoryProvider)\r" +
                    "\n                    ? immutableMagicOnionClientFactoryProvider.Add(MagicOnionGe" +
                    "neratedClientFactoryProvider.Instance)\r\n                    : new global::MagicO" +
                    "nion.Client.ImmutableMagicOnionClientFactoryProvider(MagicOnionGeneratedClientFa" +
                    "ctoryProvider.Instance);\r\n\r\n            global::MagicOnion.Client.StreamingHubCl" +
                    "ientFactoryProvider.Default =\r\n                (global::MagicOnion.Client.Stream" +
                    "ingHubClientFactoryProvider.Default is global::MagicOnion.Client.ImmutableStream" +
                    "ingHubClientFactoryProvider immutableStreamingHubClientFactoryProvider)\r\n       " +
                    "             ? immutableStreamingHubClientFactoryProvider.Add(MagicOnionGenerate" +
                    "dClientFactoryProvider.Instance)\r\n                    : new global::MagicOnion.C" +
                    "lient.ImmutableStreamingHubClientFactoryProvider(MagicOnionGeneratedClientFactor" +
                    "yProvider.Instance);\r\n        }\r\n    }\r\n\r\n    public partial class MagicOnionGen" +
                    "eratedClientFactoryProvider : global::MagicOnion.Client.IMagicOnionClientFactory" +
                    "Provider, global::MagicOnion.Client.IStreamingHubClientFactoryProvider\r\n    {\r\n " +
                    "       public static MagicOnionGeneratedClientFactoryProvider Instance { get; } " +
                    "= new MagicOnionGeneratedClientFactoryProvider();\r\n\r\n        MagicOnionGenerated" +
                    "ClientFactoryProvider() {}\r\n\r\n        bool global::MagicOnion.Client.IMagicOnion" +
                    "ClientFactoryProvider.TryGetFactory<T>(out global::MagicOnion.Client.MagicOnionC" +
                    "lientFactoryDelegate<T> factory)\r\n            => (factory = MagicOnionClientFact" +
                    "oryCache<T>.Factory) != null;\r\n\r\n        bool global::MagicOnion.Client.IStreami" +
                    "ngHubClientFactoryProvider.TryGetFactory<TStreamingHub, TReceiver>(out global::M" +
                    "agicOnion.Client.StreamingHubClientFactoryDelegate<TStreamingHub, TReceiver> fac" +
                    "tory)\r\n            => (factory = StreamingHubClientFactoryCache<TStreamingHub, T" +
                    "Receiver>.Factory) != null;\r\n\r\n        static class MagicOnionClientFactoryCache" +
                    "<T> where T : global::MagicOnion.IService<T>\r\n        {\r\n            public read" +
                    "only static global::MagicOnion.Client.MagicOnionClientFactoryDelegate<T> Factory" +
                    ";\r\n\r\n            static MagicOnionClientFactoryCache()\r\n            {\r\n         " +
                    "       object factory = default(global::MagicOnion.Client.MagicOnionClientFactor" +
                    "yDelegate<T>);\r\n\r\n");
 foreach(var serviceInfo in Services) { 
            this.Write("                if (typeof(T) == typeof(");
            this.Write(this.ToStringHelper.ToStringWithCulture(serviceInfo.ServiceType.FullName));
            this.Write("))\r\n                {\r\n                    factory = ((global::MagicOnion.Client." +
                    "MagicOnionClientFactoryDelegate<");
            this.Write(this.ToStringHelper.ToStringWithCulture(serviceInfo.ServiceType.FullName));
            this.Write(">)((x, y) => new ");
            this.Write(this.ToStringHelper.ToStringWithCulture(serviceInfo.GetClientFullName()));
            this.Write("(x, y)));\r\n                }\r\n");
 } // foreach 
            this.Write(@"                Factory = (global::MagicOnion.Client.MagicOnionClientFactoryDelegate<T>)factory;
            }
        }
        
        static class StreamingHubClientFactoryCache<TStreamingHub, TReceiver> where TStreamingHub : global::MagicOnion.IStreamingHub<TStreamingHub, TReceiver>
        {
            public readonly static global::MagicOnion.Client.StreamingHubClientFactoryDelegate<TStreamingHub, TReceiver> Factory;

            static StreamingHubClientFactoryCache()
            {
                object factory = default(global::MagicOnion.Client.StreamingHubClientFactoryDelegate<TStreamingHub, TReceiver>);

");
 foreach(var hubInfo in Hubs) { 
            this.Write("                if (typeof(TStreamingHub) == typeof(");
            this.Write(this.ToStringHelper.ToStringWithCulture(hubInfo.ServiceType.FullName));
            this.Write(") && typeof(TReceiver) == typeof(");
            this.Write(this.ToStringHelper.ToStringWithCulture(hubInfo.Receiver.ReceiverType.FullName));
            this.Write("))\r\n                {\r\n                    factory = ((global::MagicOnion.Client." +
                    "StreamingHubClientFactoryDelegate<");
            this.Write(this.ToStringHelper.ToStringWithCulture(hubInfo.ServiceType.FullName));
            this.Write(", ");
            this.Write(this.ToStringHelper.ToStringWithCulture(hubInfo.Receiver.ReceiverType.FullName));
            this.Write(">)((a, _, b, c, d, e) => new ");
            this.Write(this.ToStringHelper.ToStringWithCulture(hubInfo.GetClientFullName()));
            this.Write("(a, b, c, d, e)));\r\n                }\r\n");
 } // foreach 
            this.Write(@"
                Factory = (global::MagicOnion.Client.StreamingHubClientFactoryDelegate<TStreamingHub, TReceiver>)factory;
            }
        }
    }

}

#pragma warning restore 168
#pragma warning restore 219
#pragma warning restore 414
#pragma warning restore 612
#pragma warning restore 618
");
            return this.GenerationEnvironment.ToString();
        }
    }
    #region Base class
    /// <summary>
    /// Base class for this transformation
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "17.0.0.0")]
    public class RegisterTemplateBase
    {
        #region Fields
        private global::System.Text.StringBuilder generationEnvironmentField;
        private global::System.CodeDom.Compiler.CompilerErrorCollection errorsField;
        private global::System.Collections.Generic.List<int> indentLengthsField;
        private string currentIndentField = "";
        private bool endsWithNewline;
        private global::System.Collections.Generic.IDictionary<string, object> sessionField;
        #endregion
        #region Properties
        /// <summary>
        /// The string builder that generation-time code is using to assemble generated output
        /// </summary>
        public System.Text.StringBuilder GenerationEnvironment
        {
            get
            {
                if ((this.generationEnvironmentField == null))
                {
                    this.generationEnvironmentField = new global::System.Text.StringBuilder();
                }
                return this.generationEnvironmentField;
            }
            set
            {
                this.generationEnvironmentField = value;
            }
        }
        /// <summary>
        /// The error collection for the generation process
        /// </summary>
        public System.CodeDom.Compiler.CompilerErrorCollection Errors
        {
            get
            {
                if ((this.errorsField == null))
                {
                    this.errorsField = new global::System.CodeDom.Compiler.CompilerErrorCollection();
                }
                return this.errorsField;
            }
        }
        /// <summary>
        /// A list of the lengths of each indent that was added with PushIndent
        /// </summary>
        private System.Collections.Generic.List<int> indentLengths
        {
            get
            {
                if ((this.indentLengthsField == null))
                {
                    this.indentLengthsField = new global::System.Collections.Generic.List<int>();
                }
                return this.indentLengthsField;
            }
        }
        /// <summary>
        /// Gets the current indent we use when adding lines to the output
        /// </summary>
        public string CurrentIndent
        {
            get
            {
                return this.currentIndentField;
            }
        }
        /// <summary>
        /// Current transformation session
        /// </summary>
        public virtual global::System.Collections.Generic.IDictionary<string, object> Session
        {
            get
            {
                return this.sessionField;
            }
            set
            {
                this.sessionField = value;
            }
        }
        #endregion
        #region Transform-time helpers
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void Write(string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend))
            {
                return;
            }
            // If we're starting off, or if the previous text ended with a newline,
            // we have to append the current indent first.
            if (((this.GenerationEnvironment.Length == 0) 
                        || this.endsWithNewline))
            {
                this.GenerationEnvironment.Append(this.currentIndentField);
                this.endsWithNewline = false;
            }
            // Check if the current text ends with a newline
            if (textToAppend.EndsWith(global::System.Environment.NewLine, global::System.StringComparison.CurrentCulture))
            {
                this.endsWithNewline = true;
            }
            // This is an optimization. If the current indent is "", then we don't have to do any
            // of the more complex stuff further down.
            if ((this.currentIndentField.Length == 0))
            {
                this.GenerationEnvironment.Append(textToAppend);
                return;
            }
            // Everywhere there is a newline in the text, add an indent after it
            textToAppend = textToAppend.Replace(global::System.Environment.NewLine, (global::System.Environment.NewLine + this.currentIndentField));
            // If the text ends with a newline, then we should strip off the indent added at the very end
            // because the appropriate indent will be added when the next time Write() is called
            if (this.endsWithNewline)
            {
                this.GenerationEnvironment.Append(textToAppend, 0, (textToAppend.Length - this.currentIndentField.Length));
            }
            else
            {
                this.GenerationEnvironment.Append(textToAppend);
            }
        }
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void WriteLine(string textToAppend)
        {
            this.Write(textToAppend);
            this.GenerationEnvironment.AppendLine();
            this.endsWithNewline = true;
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void Write(string format, params object[] args)
        {
            this.Write(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void WriteLine(string format, params object[] args)
        {
            this.WriteLine(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Raise an error
        /// </summary>
        public void Error(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Raise a warning
        /// </summary>
        public void Warning(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            error.IsWarning = true;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Increase the indent
        /// </summary>
        public void PushIndent(string indent)
        {
            if ((indent == null))
            {
                throw new global::System.ArgumentNullException("indent");
            }
            this.currentIndentField = (this.currentIndentField + indent);
            this.indentLengths.Add(indent.Length);
        }
        /// <summary>
        /// Remove the last indent that was added with PushIndent
        /// </summary>
        public string PopIndent()
        {
            string returnValue = "";
            if ((this.indentLengths.Count > 0))
            {
                int indentLength = this.indentLengths[(this.indentLengths.Count - 1)];
                this.indentLengths.RemoveAt((this.indentLengths.Count - 1));
                if ((indentLength > 0))
                {
                    returnValue = this.currentIndentField.Substring((this.currentIndentField.Length - indentLength));
                    this.currentIndentField = this.currentIndentField.Remove((this.currentIndentField.Length - indentLength));
                }
            }
            return returnValue;
        }
        /// <summary>
        /// Remove any indentation
        /// </summary>
        public void ClearIndent()
        {
            this.indentLengths.Clear();
            this.currentIndentField = "";
        }
        #endregion
        #region ToString Helpers
        /// <summary>
        /// Utility class to produce culture-oriented representation of an object as a string.
        /// </summary>
        public class ToStringInstanceHelper
        {
            private System.IFormatProvider formatProviderField  = global::System.Globalization.CultureInfo.InvariantCulture;
            /// <summary>
            /// Gets or sets format provider to be used by ToStringWithCulture method.
            /// </summary>
            public System.IFormatProvider FormatProvider
            {
                get
                {
                    return this.formatProviderField ;
                }
                set
                {
                    if ((value != null))
                    {
                        this.formatProviderField  = value;
                    }
                }
            }
            /// <summary>
            /// This is called from the compile/run appdomain to convert objects within an expression block to a string
            /// </summary>
            public string ToStringWithCulture(object objectToConvert)
            {
                if ((objectToConvert == null))
                {
                    throw new global::System.ArgumentNullException("objectToConvert");
                }
                System.Type t = objectToConvert.GetType();
                System.Reflection.MethodInfo method = t.GetMethod("ToString", new System.Type[] {
                            typeof(System.IFormatProvider)});
                if ((method == null))
                {
                    return objectToConvert.ToString();
                }
                else
                {
                    return ((string)(method.Invoke(objectToConvert, new object[] {
                                this.formatProviderField })));
                }
            }
        }
        private ToStringInstanceHelper toStringHelperField = new ToStringInstanceHelper();
        /// <summary>
        /// Helper to produce culture-oriented representation of an object as a string
        /// </summary>
        public ToStringInstanceHelper ToStringHelper
        {
            get
            {
                return this.toStringHelperField;
            }
        }
        #endregion
    }
    #endregion
}
