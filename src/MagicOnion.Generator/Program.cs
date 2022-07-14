using MagicOnion.Generator;
using System;
using System.Threading.Tasks;
using ConsoleAppFramework;
using MagicOnion.Generator.Internal;
using Microsoft.Extensions.Hosting;

namespace MagicOnion.Generator
{
    public class Program : ConsoleAppBase
    {
        static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.ReplaceToSimpleConsole();
                })
                .RunConsoleAppFrameworkAsync<Program>(args)
                .ConfigureAwait(false);
        }

        public async Task RunAsync(
            [Option("i", "Input path of analyze csproj or directory.")]string input,
            [Option("o", "Output path(file) or directory base(in separated mode).")]string output,
            [Option("u", "Unuse UnityEngine's RuntimeInitializeOnLoadMethodAttribute on MagicOnionInitializer.")]bool unuseUnityAttr = false,
            [Option("n", "Set namespace root name.")]string @namespace = "MagicOnion",
            [Option("m", "Set generated MessagePackFormatter namespace.")]string messagePackGeneratedNamespace = "MessagePack.Formatters",
            [Option("c", "Conditional compiler symbols, split with ','.")]string conditionalSymbol = null,
            [Option("v", "Enable verbose logging")]bool verbose = false)
        {
            await new MagicOnionCompiler(new MagicOnionGeneratorConsoleLogger(verbose), this.Context.CancellationToken)
                .GenerateFileAsync(
                    input,
                    output,
                    unuseUnityAttr,
                    @namespace,
                    conditionalSymbol,
                    messagePackGeneratedNamespace);
        }
    }
}
