using System.ComponentModel;
using JetBrains.Annotations;

namespace mazharenko.AoCAgent;

[EditorBrowsable(EditorBrowsableState.Never)]
[UsedImplicitly]
public record RunnerDay(int Num, RunnerPart Part1, RunnerPart Part2);