using MediatR;
using OmniSharp.Extensions.DebugAdapter.Protocol;
using OmniSharp.Extensions.DebugAdapter.Protocol.Models;
using OmniSharp.Extensions.DebugAdapter.Protocol.Requests;
using OmniSharp.Extensions.DebugAdapter.Protocol.Server;
using OmniSharp.Extensions.JsonRpc;
using Spect.Net.SpectrumEmu.Machine;
using System.Threading;
using System.Threading.Tasks;

namespace Spect.Net.Dap.Handlers;

[Method(RequestNames.Pause)]
public class PauseHandler : IJsonRpcRequestHandler<PauseArguments, PauseResponse>
{
    private readonly SpectNetDebugSession _debugSession;

    public PauseHandler(SpectNetDebugSession debugSession)
    {
        _debugSession = debugSession;
    }

    public async Task<PauseResponse> Handle(PauseArguments request, CancellationToken cancellationToken)
    {
        if (_debugSession.Machine != null)
        {
            await _debugSession.Machine.Pause();
        }
        return new PauseResponse();
    }
}

[Method(RequestNames.Continue)]
public class ContinueHandler : IJsonRpcRequestHandler<ContinueArguments, ContinueResponse>
{
    private readonly SpectNetDebugSession _debugSession;

    public ContinueHandler(SpectNetDebugSession debugSession)
    {
        _debugSession = debugSession;
    }

    public Task<ContinueResponse> Handle(ContinueArguments request, CancellationToken cancellationToken)
    {
        if (_debugSession.Machine != null)
        {
            _debugSession.Machine.Start(new ExecuteCycleOptions(EmulationMode.Continuous));
        }
        return Task.FromResult(new ContinueResponse { AllThreadsContinued = true });
    }
}

[Method(RequestNames.Next)]
public class NextHandler : IJsonRpcRequestHandler<NextArguments, NextResponse>
{
    private readonly SpectNetDebugSession _debugSession;

    public NextHandler(SpectNetDebugSession debugSession)
    {
        _debugSession = debugSession;
    }

    public Task<NextResponse> Handle(NextArguments request, CancellationToken cancellationToken)
    {
        if (_debugSession.Machine != null)
        {
            // Step Over
            _debugSession.Machine.Start(new ExecuteCycleOptions(EmulationMode.Debugger, DebugStepMode.StepOver));
        }
        return Task.FromResult(new NextResponse());
    }
}

[Method(RequestNames.StepIn)]
public class StepInHandler : IJsonRpcRequestHandler<StepInArguments, StepInResponse>
{
    private readonly SpectNetDebugSession _debugSession;

    public StepInHandler(SpectNetDebugSession debugSession)
    {
        _debugSession = debugSession;
    }

    public Task<StepInResponse> Handle(StepInArguments request, CancellationToken cancellationToken)
    {
        if (_debugSession.Machine != null)
        {
            // Step Into (Single Step)
            _debugSession.Machine.Start(new ExecuteCycleOptions(EmulationMode.Debugger, DebugStepMode.StepInto));
        }
        return Task.FromResult(new StepInResponse());
    }
}

[Method(RequestNames.StepOut)]
public class StepOutHandler : IJsonRpcRequestHandler<StepOutArguments, StepOutResponse>
{
    private readonly SpectNetDebugSession _debugSession;

    public StepOutHandler(SpectNetDebugSession debugSession)
    {
        _debugSession = debugSession;
    }

    public Task<StepOutResponse> Handle(StepOutArguments request, CancellationToken cancellationToken)
    {
        if (_debugSession.Machine != null)
        {
            // Step Out
             _debugSession.Machine.Start(new ExecuteCycleOptions(EmulationMode.Debugger, DebugStepMode.StepOut));
        }
        return Task.FromResult(new StepOutResponse());
    }
}
