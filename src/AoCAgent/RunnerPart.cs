using System.ComponentModel;
using JetBrains.Annotations;
using mazharenko.AoCAgent.Base;

namespace mazharenko.AoCAgent;

[EditorBrowsable(EditorBrowsableState.Never)]
[UsedImplicitly]
public record RunnerPart(int Num, IPart Part);