using OmniSharp.Extensions.DebugAdapter.Protocol.Requests;
using OmniSharp.Extensions.DebugAdapter.Protocol.Server;
using System.Threading.Tasks;
using System.Threading;

namespace Spect.Net.Dap.Handlers;

public class AttachHandler : IAttachHandler
{
    public Task<AttachResponse> Handle(AttachRequestArguments request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new AttachResponse());
    }
}
