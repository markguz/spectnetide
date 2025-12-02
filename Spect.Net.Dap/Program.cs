using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.DebugAdapter.Server;
using MediatR;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.DebugAdapter.Protocol;
using OmniSharp.Extensions.DebugAdapter.Protocol.Requests;
using Spect.Net.Dap.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Spect.Net.Dap;

class Program
{
    static async Task Main(string[] args)
    {
        var server = await OmniSharp.Extensions.DebugAdapter.Server.DebugAdapterServer.From(options =>
            options
                .WithInput(Console.OpenStandardInput())
                .WithOutput(Console.OpenStandardOutput())
                .WithServices(services =>
                {
                    services.AddSingleton<SpectNetDebugSession>();
                    services.AddSingleton<IJsonRpcHandler, LaunchHandler>();
                    services.AddSingleton<IJsonRpcHandler, AttachHandler>();
                    services.AddSingleton<IJsonRpcHandler, SetBreakpointsHandler>();
                    services.AddSingleton<IJsonRpcHandler, ThreadsHandler>();
                    services.AddSingleton<IJsonRpcHandler, StackTraceHandler>();
                    services.AddSingleton<IJsonRpcHandler, ScopesHandler>();
                    services.AddSingleton<IJsonRpcHandler, VariablesHandler>();
                })
                .ConfigureLogging(x => x
                    //.AddDebugAdapterProtocolLogging()
                    .SetMinimumLevel(LogLevel.Debug)
                )
        );

        await Task.Delay(-1);
    }
}
