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
	string SolveString(string input);
	
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