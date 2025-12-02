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

[Method(RequestNames.Threads)]
public class ThreadsHandler : IJsonRpcRequestHandler<ThreadsArguments, ThreadsResponse>
{
    public Task<ThreadsResponse> Handle(ThreadsArguments request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ThreadsResponse
        {
            Threads = new List<OmniSharp.Extensions.DebugAdapter.Protocol.Models.Thread>
            {
                new OmniSharp.Extensions.DebugAdapter.Protocol.Models.Thread
                {
                    Id = 1,
                    Name = "Main Thread"
                }
            }
        });
    }
}
