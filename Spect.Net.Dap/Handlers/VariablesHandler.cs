using MediatR;
using OmniSharp.Extensions.DebugAdapter.Protocol;
using OmniSharp.Extensions.DebugAdapter.Protocol.Models;
using OmniSharp.Extensions.DebugAdapter.Protocol.Requests;
using OmniSharp.Extensions.DebugAdapter.Protocol.Server;
using OmniSharp.Extensions.JsonRpc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Spect.Net.Dap.Handlers;

[Method(RequestNames.Variables)]
public class VariablesHandler : IJsonRpcRequestHandler<VariablesArguments, VariablesResponse>
{
    private readonly SpectNetDebugSession _debugSession;

    public VariablesHandler(SpectNetDebugSession debugSession)
    {
        _debugSession = debugSession;
    }

    public Task<VariablesResponse> Handle(VariablesArguments request, CancellationToken cancellationToken)
    {
        var variables = new List<Variable>();

        if (request.VariablesReference == 1)
        {
            var regs = _debugSession.Machine.SpectrumVm.Cpu.Registers;
            variables.Add(new Variable { Name = "AF", Value = $"0x{regs.AF:X4}", VariablesReference = 0 });
            variables.Add(new Variable { Name = "BC", Value = $"0x{regs.BC:X4}", VariablesReference = 0 });
            variables.Add(new Variable { Name = "DE", Value = $"0x{regs.DE:X4}", VariablesReference = 0 });
            variables.Add(new Variable { Name = "HL", Value = $"0x{regs.HL:X4}", VariablesReference = 0 });
            variables.Add(new Variable { Name = "PC", Value = $"0x{regs.PC:X4}", VariablesReference = 0 });
            variables.Add(new Variable { Name = "SP", Value = $"0x{regs.SP:X4}", VariablesReference = 0 });
            variables.Add(new Variable { Name = "IX", Value = $"0x{regs.IX:X4}", VariablesReference = 0 });
            variables.Add(new Variable { Name = "IY", Value = $"0x{regs.IY:X4}", VariablesReference = 0 });
            variables.Add(new Variable { Name = "I", Value = $"0x{regs.I:X2}", VariablesReference = 0 });
            variables.Add(new Variable { Name = "R", Value = $"0x{regs.R:X2}", VariablesReference = 0 });
            variables.Add(new Variable { Name = "AF'", Value = $"0x{regs._AF_:X4}", VariablesReference = 0 });
            variables.Add(new Variable { Name = "BC'", Value = $"0x{regs._BC_:X4}", VariablesReference = 0 });
            variables.Add(new Variable { Name = "DE'", Value = $"0x{regs._DE_:X4}", VariablesReference = 0 });
            variables.Add(new Variable { Name = "HL'", Value = $"0x{regs._HL_:X4}", VariablesReference = 0 });
        }

        return Task.FromResult(new VariablesResponse
        {
            Variables = variables
        });
    }
}
