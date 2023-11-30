using System.ComponentModel;
using JetBrains.Annotations;

namespace mazharenko.AoCAgent.Base;

[EditorBrowsable(EditorBrowsableState.Never)]
[UsedImplicitly]
public interface IPart
{
	[UsedImplicitly]
	IEnumerable<NamedExample> GetExamples();
	
	[UsedImplicitly]
	string SolveObtained(string input);
	
	[UsedImplicitly]
	Settings Settings { get; }
}

[EditorBrowsable(EditorBrowsableState.Never)]
[UsedImplicitly]
public class Settings
{
	[UsedImplicitly]
	public required bool BypassNoExamples { get; init; }
}

[EditorBrowsable(EditorBrowsableState.Never)]
[UsedImplicitly]
public interface IPart<TInput, TRes>
{
	[UsedImplicitly]
	TInput ParseObtained(string input);
	
	[UsedImplicitly]
	TRes Solve(TInput input);
}


[EditorBrowsable(EditorBrowsableState.Never)]
[UsedImplicitly]
public abstract class PartBase : IPart
{
	[UsedImplicitly]
	public abstract IEnumerable<NamedExample> GetExamples();
	
	[UsedImplicitly]
	public abstract string SolveObtained(string input);

	[UsedImplicitly]
	public abstract Settings Settings { get; }
}

[EditorBrowsable(EditorBrowsableState.Never)]
[UsedImplicitly]
public abstract class PartBase<TInput, TRes> : PartBase
{
	[UsedImplicitly]
	public virtual string Format(TRes res) => res!.ToString()!;
}
