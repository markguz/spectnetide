using OmniSharp.Extensions.DebugAdapter.Protocol.Requests;
using OmniSharp.Extensions.DebugAdapter.Protocol.Server;
using System.Threading.Tasks;
using System.Threading;

namespace Spect.Net.Dap.Handlers;

public class LaunchHandler : ILaunchHandler
{
    public Task<LaunchResponse> Handle(LaunchRequestArguments request, CancellationToken cancellationToken)
    {
        // TODO: Initialize Emulator and Load Program
        return Task.FromResult(new LaunchResponse());
    }
}
