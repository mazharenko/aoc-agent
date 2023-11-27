using System.ComponentModel;
using JetBrains.Annotations;

namespace mazharenko.AoCAgent.Base;

[UsedImplicitly]
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class YearBase
{
	[UsedImplicitly]
	public abstract int Year { get; }
	[UsedImplicitly]
	public readonly IList<RunnerDay> Days = new List<RunnerDay>();
}