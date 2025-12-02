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

[Method(RequestNames.Scopes)]
public class ScopesHandler : IJsonRpcRequestHandler<ScopesArguments, ScopesResponse>
{
    public Task<ScopesResponse> Handle(ScopesArguments request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ScopesResponse
        {
            Scopes = new List<Scope>
            {
                new Scope
                {
                    Name = "Registers",
                    VariablesReference = 1, // ID for Registers
                    Expensive = false
                }
            }
        });
    }
}
