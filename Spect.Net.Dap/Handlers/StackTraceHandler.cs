using MediatR;
using OmniSharp.Extensions.DebugAdapter.Protocol;
using OmniSharp.Extensions.DebugAdapter.Protocol.Models;
using OmniSharp.Extensions.DebugAdapter.Protocol.Requests;
using OmniSharp.Extensions.DebugAdapter.Protocol.Server;
using OmniSharp.Extensions.JsonRpc;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Spect.Net.Dap.Handlers;

[Method(RequestNames.StackTrace)]
public class StackTraceHandler : IJsonRpcRequestHandler<StackTraceArguments, StackTraceResponse>
{
    private readonly SpectNetDebugSession _debugSession;

    public StackTraceHandler(SpectNetDebugSession debugSession)
    {
        _debugSession = debugSession;
    }

    public Task<StackTraceResponse> Handle(StackTraceArguments request, CancellationToken cancellationToken)
    {
        var stackFrames = new List<StackFrame>();
        var pc = _debugSession.Machine.SpectrumVm.Cpu.Registers.PC;
        
        // Map PC to Source
        string sourcePath = null;
        int line = 0;
        string name = $"0x{pc:X4}";

        if (_debugSession.AssemblerOutput != null && _debugSession.AssemblerOutput.SourceMap.TryGetValue(pc, out var sourceInfo))
        {
            if (sourceInfo.FileIndex >= 0 && sourceInfo.FileIndex < _debugSession.AssemblerOutput.SourceFileList.Count)
            {
                var fileItem = _debugSession.AssemblerOutput.SourceFileList[sourceInfo.FileIndex];
                sourcePath = fileItem.Filename;
                line = sourceInfo.Line;
                name = $"{Path.GetFileName(sourcePath)}:{line}";
            }
        }

        stackFrames.Add(new StackFrame
        {
            Id = 0,
            Name = name,
            Line = line,
            Column = 0,
            Source = sourcePath != null ? new Source { Path = sourcePath } : null,
            PresentationHint = StackFramePresentationHint.Normal
        });

        return Task.FromResult(new StackTraceResponse
        {
            StackFrames = stackFrames,
            TotalFrames = 1
        });
    }
}
