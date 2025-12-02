using Spect.Net.EvalParser.SyntaxTree;
using Spect.Net.SpectrumEmu.Machine;

namespace Spect.Net.Dap.Models;

public class DapBreakpointInfo : IBreakpointInfo
{
    public bool IsCpuBreakpoint { get; set; } = true;
    public BreakpointHitType HitType { get; set; } = BreakpointHitType.None;
    public ushort HitConditionValue { get; set; }
    public string FilterCondition { get; set; }
    public ExpressionNode FilterExpression { get; set; }
    public int CurrentHitCount { get; set; }
}
