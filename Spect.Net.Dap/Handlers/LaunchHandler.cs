using MediatR;
using OmniSharp.Extensions.DebugAdapter.Protocol;
using OmniSharp.Extensions.DebugAdapter.Protocol.Requests;
using OmniSharp.Extensions.DebugAdapter.Protocol.Server;
using OmniSharp.Extensions.JsonRpc;
using Spect.Net.Assembler.Assembler;
using Spect.Net.Dap.Providers;
using Spect.Net.SpectrumEmu.Abstraction.Models;
using Spect.Net.SpectrumEmu.Abstraction.Providers;
using Spect.Net.SpectrumEmu.Machine;
using Spect.Net.SpectrumEmu;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Spect.Net.Dap.Handlers;

[Method(RequestNames.Launch)]
public class LaunchHandler : IJsonRpcRequestHandler<SpectNetLaunchArguments, LaunchResponse>
{
    private readonly SpectNetDebugSession _debugSession;

    public LaunchHandler(SpectNetDebugSession debugSession)
    {
        _debugSession = debugSession;
    }

    public Task<LaunchResponse> Handle(SpectNetLaunchArguments request, CancellationToken cancellationToken)
    {
        // 1. Compile the program
        var asmSource = File.ReadAllText(request.Program);
        var assembler = new Z80Assembler();
        var output = assembler.Compile(asmSource);

        if (output.ErrorCount > 0)
        {
            // TODO: Report errors
            return Task.FromResult(new LaunchResponse());
        }

        // 2. Initialize Machine
        SpectrumMachine.Reset();
        SpectrumMachine.RegisterProvider<IRomProvider>(() => new DapRomProvider());
        SpectrumMachine.RegisterProvider<IKeyboardProvider>(() => new DapKeyboardProvider());
        SpectrumMachine.RegisterProvider<IBeeperProvider>(() => new DapBeeperProvider());
        SpectrumMachine.RegisterProvider<ITapeProvider>(() => new DapTapeProvider());
        SpectrumMachine.RegisterProvider<IKempstonProvider>(() => new DapKempstonProvider());
        SpectrumMachine.RegisterProvider<ISpectrumDebugInfoProvider>(() => new DapDebugInfoProvider());

        var machine = SpectrumMachine.CreateMachine(SpectrumModels.ZX_SPECTRUM_48, SpectrumModels.PAL);
        _debugSession.Machine = machine;

        // 3. Inject Code
        foreach (var segment in output.Segments)
        {
            var addr = segment.StartAddress;
            foreach (var b in segment.EmittedCode)
            {
                machine.SpectrumVm.MemoryDevice.Write(addr++, b);
            }
        }
        
        // Set entry point if available (e.g. from 'ent' directive or default)
        if (output.EntryAddress != null)
        {
             machine.SpectrumVm.Cpu.Registers.PC = output.EntryAddress.Value;
        }

        // 4. Start Machine
        // We start it in a background thread, but we need to handle the execution loop.
        // For DAP, we usually want to run until a breakpoint or pause.
        // SpectrumMachine.Start runs in a separate thread.
        
        machine.ExecuteOnMainThread = (action) => 
        {
            action();
            return Task.CompletedTask;
        };

        machine.Start(new ExecuteCycleOptions(EmulationMode.Continuous));

        return Task.FromResult(new LaunchResponse());
    }
}
