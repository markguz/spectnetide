using OmniSharp.Extensions.DebugAdapter.Protocol.Server;
using OmniSharp.Extensions.DebugAdapter.Protocol.Events;
using OmniSharp.Extensions.DebugAdapter.Protocol.Models;
using Spect.Net.Assembler.Assembler;
using Spect.Net.SpectrumEmu.Machine;
using Spect.Net.SpectrumEmu.Devices.Tape;
using Spect.Net.SpectrumEmu.Devices.Tape.Tzx;
using Spect.Net.SpectrumEmu.Disassembler;
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
        Machine.VmScreenRefreshed += OnVmScreenRefreshed;
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
                SendRegisters();
                SendDisassembly();
                SendMemory();
                SendTapeInfo();
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

    private void SendRegisters()
    {
        var regs = Machine.SpectrumVm.Cpu.Registers;
        var data = new
        {
            AF = $"0x{regs.AF:X4}",
            BC = $"0x{regs.BC:X4}",
            DE = $"0x{regs.DE:X4}",
            HL = $"0x{regs.HL:X4}",
            PC = $"0x{regs.PC:X4}",
            SP = $"0x{regs.SP:X4}",
            IX = $"0x{regs.IX:X4}",
            IY = $"0x{regs.IY:X4}",
            I = $"0x{regs.I:X2}",
            R = $"0x{regs.R:X2}",
            AF_ = $"0x{regs._AF_:X4}",
            BC_ = $"0x{regs._BC_:X4}",
            DE_ = $"0x{regs._DE_:X4}",
            HL_ = $"0x{regs._HL_:X4}"
        };
        _debugAdapterServer.SendNotification("spectnet/registers", data);
    }

    private void SendDisassembly()
    {
        var pc = Machine.SpectrumVm.Cpu.Registers.PC;
        var memoryContents = Machine.SpectrumVm.MemoryDevice.CloneMemory();
        var memorySections = new List<MemorySection>
        {
            new MemorySection(0x0000, 0xFFFF, MemorySectionType.Disassemble)
        };
        var disassembler = new Z80Disassembler(memorySections, memoryContents);
        var output = disassembler.Disassemble(pc, (ushort)(pc + 100)); // Disassemble next 100 bytes approx

        var items = new List<object>();
        foreach (var item in output.OutputItems)
        {
            items.Add(new
            {
                address = item.Address,
                addressHex = $"0x{item.Address:X4}",
                opCodes = item.OpCodes,
                instruction = item.Instruction
            });
        }

        _debugAdapterServer.SendNotification("spectnet/disassembly", new { pc = pc, items = items });
    }

    private void SendMemory()
    {
        var memoryContents = Machine.SpectrumVm.MemoryDevice.CloneMemory();
        var base64 = Convert.ToBase64String(memoryContents);
        _debugAdapterServer.SendNotification("spectnet/memory", new { startAddress = 0x0000, memory = base64 });
    }

    private void SendTapeInfo()
    {
        var tapeDevice = Machine.SpectrumVm.TapeDevice as TapeDevice;
        if (tapeDevice?.TapeFilePlayer == null)
        {
            _debugAdapterServer.SendNotification("spectnet/tape", new object[0]);
            return;
        }

        var blocks = new List<object>();
        var currentIndex = tapeDevice.TapeFilePlayer.CurrentBlockIndex;

        foreach (var block in tapeDevice.TapeFilePlayer.DataBlocks)
        {
            var info = new
            {
                type = "Block",
                name = "",
                size = 0,
                start = (int?)null,
                param2 = (int?)null,
                isCurrent = false // Will set below
            };

            if (block is TzxStandardSpeedDataBlock stdBlock)
            {
                info = new
                {
                    type = "Standard Speed",
                    name = "",
                    size = (int)stdBlock.DataLength,
                    start = (int?)null,
                    param2 = (int?)null,
                    isCurrent = false
                };

                if (stdBlock.DataLength == 19 && stdBlock.Data[0] == 0x00) // Header
                {
                    var typeId = stdBlock.Data[1];
                    var typeStr = typeId switch
                    {
                        0 => "Program",
                        1 => "Number Array",
                        2 => "Char Array",
                        3 => "Bytes",
                        _ => "Unknown"
                    };

                    var name = System.Text.Encoding.ASCII.GetString(stdBlock.Data, 2, 10).Trim();
                    var len = stdBlock.Data[12] + stdBlock.Data[13] * 256;
                    var start = stdBlock.Data[14] + stdBlock.Data[15] * 256;
                    var p2 = stdBlock.Data[16] + stdBlock.Data[17] * 256;

                    info = new
                    {
                        type = $"Header ({typeStr})",
                        name = name,
                        size = len,
                        start = (int?)start,
                        param2 = (int?)p2,
                        isCurrent = false
                    };
                }
                else if (stdBlock.Data[0] == 0xFF) // Data
                {
                    info = new
                    {
                        type = "Data",
                        name = "",
                        size = (int)stdBlock.DataLength - 2, // Exclude flag and checksum
                        start = (int?)null,
                        param2 = (int?)null,
                        isCurrent = false
                    };
                }
            }
            // Add other block types here if needed

            blocks.Add(info);
        }

        // Mark current block
        if (currentIndex >= 0 && currentIndex < blocks.Count)
        {
             // We can't easily modify anonymous objects, so we'll just send the index separately or handle it in JS.
             // Actually, let's just recreate the object or use a dictionary if we want to be cleaner, 
             // but for now, let's just send the list and the current index.
             // Wait, I can't modify the anonymous object. 
             // Let's change the strategy: Send the list AND the current index.
        }
        
        // Re-thinking: Let's just send the list and the current index in the wrapper object.
        _debugAdapterServer.SendNotification("spectnet/tape", new { blocks = blocks, currentIndex = currentIndex });
    }

    private void OnVmScreenRefreshed(object? sender, VmScreenRefreshedEventArgs e)
    {
        var base64 = Convert.ToBase64String(e.Buffer);
        _debugAdapterServer.SendNotification("spectnet/videoFrame", new { data = base64 });
    }
    
    public void Start()
    {
        // Logic to start the machine
    }
}
