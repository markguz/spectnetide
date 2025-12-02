using OmniSharp.Extensions.DebugAdapter.Protocol.Server;
using OmniSharp.Extensions.DebugAdapter.Protocol.Events;
using OmniSharp.Extensions.DebugAdapter.Protocol.Models;
using Spect.Net.Assembler.Assembler;
using Spect.Net.SpectrumEmu.Machine;
using Spect.Net.Dap.Handlers;
using System;

namespace Spect.Net.Dap;

public class SpectNetDebugSession
{
    private readonly IDebugAdapterServer _debugAdapterServer;

    public SpectNetDebugSession(IDebugAdapterServer debugAdapterServer)
    {
        _debugAdapterServer = debugAdapterServer;
    }

    public SpectrumMachine? Machine { get; private set; }
    public AssemblerOutput? AssemblerOutput { get; set; }
    public SpectNetLaunchArguments? LaunchArguments { get; set; }

    public void SetMachine(SpectrumMachine machine)
    {
        Machine = machine;
        Machine.VmStateChanged += OnVmStateChanged;
    }

    private void OnVmStateChanged(object? sender, VmStateChangedEventArgs e)
    {
        switch (e.NewState)
        {
            case VmState.Paused:
                _debugAdapterServer.SendNotification(new StoppedEvent
                {
                    Reason = StoppedEventReason.Pause,
                    ThreadId = 1,
                    AllThreadsStopped = true
                });
                break;
            
            case VmState.Running:
                _debugAdapterServer.SendNotification(new ContinuedEvent
                {
                    ThreadId = 1,
                    AllThreadsContinued = true
                });
                break;
                
            case VmState.Stopped:
                _debugAdapterServer.SendNotification(new TerminatedEvent());
                break;
        }
    }
    
    public void Start()
    {
        // Logic to start the machine
    }
}
