using Spect.Net.SpectrumEmu.Abstraction.Providers;
using Spect.Net.SpectrumEmu.Machine;

namespace Spect.Net.Dap.Providers;

public class DapDebugInfoProvider : VmComponentProviderBase, ISpectrumDebugInfoProvider
{
    public BreakpointCollection Breakpoints { get; } = new BreakpointCollection();
    public ushort? ImminentBreakpoint { get; set; }

    public override void Reset()
    {
        Breakpoints.Clear();
        ImminentBreakpoint = null;
    }

    public void PrepareBreakpoints()
    {
        // No special preparation needed for now
    }

    public void ResetHitCounts()
    {
        foreach (var bp in Breakpoints)
        {
            bp.Value.CurrentHitCount = 0;
        }
    }

    public bool ShouldBreakAtAddress(ushort address)
    {
        if (ImminentBreakpoint.HasValue && ImminentBreakpoint.Value == address)
        {
            return true;
        }

        if (Breakpoints.TryGetValue(address, out var bp))
        {
            bp.CurrentHitCount++;
            var val = bp.HitConditionValue;
            return bp.HitType switch
            {
                BreakpointHitType.None => true,
                BreakpointHitType.Less => bp.CurrentHitCount < val,
                BreakpointHitType.LessOrEqual => bp.CurrentHitCount <= val,
                BreakpointHitType.Equal => bp.CurrentHitCount == val,
                BreakpointHitType.Greater => bp.CurrentHitCount > val,
                BreakpointHitType.GreaterOrEqual => bp.CurrentHitCount >= val,
                BreakpointHitType.Multiple => bp.CurrentHitCount % val == 0,
                _ => true
            };
        }
        return false;
    }
}
