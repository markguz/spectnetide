using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.DebugAdapter.Server;
using OmniSharp.Extensions.DebugAdapter.Protocol;
using OmniSharp.Extensions.DebugAdapter.Protocol.Requests;
using Spect.Net.Dap.Handlers;

namespace Spect.Net.Dap;

class Program
{
    static async Task Main(string[] args)
    {
        var server = await OmniSharp.Extensions.DebugAdapter.Server.DebugAdapterServer.From(options =>
            options
                .WithInput(Console.OpenStandardInput())
                .WithOutput(Console.OpenStandardOutput())
                .WithHandler<LaunchHandler>()
                .WithHandler<AttachHandler>()
                .ConfigureLogging(x => x
                    //.AddDebugAdapterProtocolLogging()
                    .SetMinimumLevel(LogLevel.Debug)
                )
        );

        await Task.Delay(-1);
    }
}
