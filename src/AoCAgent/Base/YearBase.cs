using System.ComponentModel;
using JetBrains.Annotations;

namespace mazharenko.AoCAgent.Base;

[UsedImplicitly]
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class YearBase
{
	public abstract int MaxStars { get; }
	public abstract int MaxDays { get; }
	[UsedImplicitly]
	public abstract int Year { get; }
	[UsedImplicitly]
	public readonly IList<RunnerPart> Parts = new List<RunnerPart>();
}