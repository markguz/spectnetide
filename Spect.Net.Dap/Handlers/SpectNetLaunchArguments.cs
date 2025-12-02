using OmniSharp.Extensions.DebugAdapter.Protocol.Requests;
using System.Collections.Generic;

namespace Spect.Net.Dap.Handlers;

public record SpectNetLaunchArguments : LaunchRequestArguments
{
    public string Program { get; init; }
    public bool StopOnEntry { get; init; }
    public string? Model { get; init; }
    public string? Edition { get; init; }
    public List<string>? PredefinedSymbols { get; init; }
    public ushort? DefaultStartAddress { get; init; }
    public int? DefaultDisplacement { get; init; }
}
