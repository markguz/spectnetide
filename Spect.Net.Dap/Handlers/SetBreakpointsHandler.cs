using MediatR;
using OmniSharp.Extensions.DebugAdapter.Protocol;
using OmniSharp.Extensions.DebugAdapter.Protocol.Models;
using OmniSharp.Extensions.DebugAdapter.Protocol.Requests;
using OmniSharp.Extensions.DebugAdapter.Protocol.Server;
using OmniSharp.Extensions.JsonRpc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Spect.Net.Dap.Providers;
using Spect.Net.Dap.Models;

namespace Spect.Net.Dap.Handlers;

[Method(RequestNames.SetBreakpoints)]
public class SetBreakpointsHandler : IJsonRpcRequestHandler<SetBreakpointsArguments, SetBreakpointsResponse>
{
    private readonly SpectNetDebugSession _debugSession;

    public SetBreakpointsHandler(SpectNetDebugSession debugSession)
    {
        _debugSession = debugSession;
    }

    public Task<SetBreakpointsResponse> Handle(SetBreakpointsArguments request, CancellationToken cancellationToken)
    {
        var breakpoints = new List<Breakpoint>();

        if (_debugSession.AssemblerOutput == null || request.Source?.Path == null)
        {
            return Task.FromResult(new SetBreakpointsResponse { Breakpoints = breakpoints });
        }

        // Find FileIndex
        var fileIndex = -1;
        var requestPath = Path.GetFullPath(request.Source.Path);
        
        for (int i = 0; i < _debugSession.AssemblerOutput.SourceFileList.Count; i++)
        {
            var item = _debugSession.AssemblerOutput.SourceFileList[i];
            // Handle potential null filenames in SourceFileList (though unlikely for valid files)
            if (item.Filename != null && string.Equals(Path.GetFullPath(item.Filename), requestPath, StringComparison.OrdinalIgnoreCase))
            {
                fileIndex = i;
                break;
            }
        }

        if (fileIndex == -1)
        {
            // File not found in compilation
            if (request.Breakpoints != null)
            {
                foreach (var bp in request.Breakpoints)
                {
                    breakpoints.Add(new Breakpoint { Verified = false, Message = "File not part of compilation" });
                }
            }
            return Task.FromResult(new SetBreakpointsResponse { Breakpoints = breakpoints });
        }

        // Clear existing breakpoints for this file? 
        var addressesToRemove = new List<ushort>();
        foreach (var entry in _debugSession.AssemblerOutput.AddressMap)
        {
            if (entry.Key.FileIndex == fileIndex)
            {
                addressesToRemove.AddRange(entry.Value);
            }
        }
        
        foreach (var addr in addressesToRemove)
        {
            _debugSession.Machine.SpectrumVm.DebugInfoProvider.Breakpoints.Remove(addr);
        }

        // Set new breakpoints
        if (request.Breakpoints != null)
        {
            foreach (var sourceBp in request.Breakpoints)
            {
                var line = (int)sourceBp.Line; // DAP is 1-based? VS Code is 1-based. AssemblerOutput is likely 1-based.
                
                if (_debugSession.AssemblerOutput.AddressMap.TryGetValue((fileIndex, line), out var addresses))
                {
                    foreach (var addr in addresses)
                    {
                        _debugSession.Machine.SpectrumVm.DebugInfoProvider.Breakpoints.Add(addr, new DapBreakpointInfo
                        {
                            HitConditionValue = 0, // Default
                            IsCpuBreakpoint = true
                        });
                    }

                    breakpoints.Add(new Breakpoint
                    {
                        Verified = true,
                        Line = line,
                        Source = request.Source
                    });
                }
                else
                {
                     breakpoints.Add(new Breakpoint
                    {
                        Verified = false,
                        Line = line,
                        Message = "No code at this line"
                    });
                }
            }
        }

        return Task.FromResult(new SetBreakpointsResponse { Breakpoints = breakpoints });
    }
}
