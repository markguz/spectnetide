using OmniSharp.Extensions.DebugAdapter.Protocol.Requests;

namespace Spect.Net.Dap.Handlers;

public record SpectNetLaunchArguments : LaunchRequestArguments
{
    public string Program { get; init; }
    public bool StopOnEntry { get; init; }
    public string? Model { get; init; }
    public string? Edition { get; init; }
}
