using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Server;
using Spect.Net.LanguageServer.Services;

namespace Spect.Net.LanguageServer;

class Program
{
    static async Task Main(string[] args)
    {
        var server = await OmniSharp.Extensions.LanguageServer.Server.LanguageServer.From(options =>
            options
                .WithInput(Console.OpenStandardInput())
                .WithOutput(Console.OpenStandardOutput())
                .ConfigureLogging(x => x
                    .AddLanguageProtocolLogging()
                    .SetMinimumLevel(LogLevel.Debug)
                )
                .WithHandler<Handlers.TextDocumentHandler>()
                .WithHandler<Handlers.HoverHandler>()
                .WithHandler<Handlers.DefinitionHandler>()
                .WithServices(x =>
                {
                    x.AddLogging(b => b.SetMinimumLevel(LogLevel.Debug));
                    x.AddSingleton<Z80CompilationCache>();
                })
        );

        await server.WaitForExit;
    }
}
