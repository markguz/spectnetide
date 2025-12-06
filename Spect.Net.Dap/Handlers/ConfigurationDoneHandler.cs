using MediatR;
using OmniSharp.Extensions.DebugAdapter.Protocol;
using OmniSharp.Extensions.DebugAdapter.Protocol.Events;
using OmniSharp.Extensions.DebugAdapter.Protocol.Models;
using OmniSharp.Extensions.DebugAdapter.Protocol.Requests;
using OmniSharp.Extensions.DebugAdapter.Protocol.Server;
using OmniSharp.Extensions.JsonRpc;
using Spect.Net.SpectrumEmu.Machine;
using System.Threading;
using System.Threading.Tasks;

namespace Spect.Net.Dap.Handlers;

[Method(RequestNames.ConfigurationDone)]
public class ConfigurationDoneHandler : IJsonRpcRequestHandler<ConfigurationDoneArguments, ConfigurationDoneResponse>
{
    private readonly SpectNetDebugSession _debugSession;
    
    public ConfigurationDoneHandler(SpectNetDebugSession debugSession)
    {
        _debugSession = debugSession;
    }

    public Task<ConfigurationDoneResponse> Handle(ConfigurationDoneArguments request, CancellationToken cancellationToken)
    {
        if (_debugSession.Machine != null)
        {
            var stopOnEntry = _debugSession.LaunchArguments?.StopOnEntry ?? false;
            var noDebug = _debugSession.LaunchArguments?.NoDebug ?? false;
            
            if (stopOnEntry && !noDebug)
            {
                // Start and immediately pause at entry point.
                // We use UntilExecutionPoint with the current PC (Entry Address)
                var pc = _debugSession.Machine.SpectrumVm.Cpu.Registers.PC;
                _debugSession.Machine.Start(new ExecuteCycleOptions(EmulationMode.UntilExecutionPoint, terminationPoint: pc));
            }
            else
            {
                // Start continuous execution
                _debugSession.Machine.Start(new ExecuteCycleOptions(EmulationMode.Continuous));
            }
        }

        return Task.FromResult(new ConfigurationDoneResponse());
    }
}
