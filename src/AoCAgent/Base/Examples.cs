using System.ComponentModel;
using JetBrains.Annotations;

namespace mazharenko.AoCAgent.Base;

[EditorBrowsable(EditorBrowsableState.Never)]
[UsedImplicitly]
public record NamedExample(string Name, IExample Example);

[EditorBrowsable(EditorBrowsableState.Never)]
[UsedImplicitly]
public interface IExample
{
	object Expectation { get; }
	string ExpectationFormatted { get; }
	object Run();
	object RunFormat(out string formatted);
}

[EditorBrowsable(EditorBrowsableState.Never)]
[UsedImplicitly]
public record DayExample<TInput>(TInput Input) : IExample
{
	private IExample PartExample
	{
		get
		{
			if (field is null)
				throw new InvalidOperationException("Example is not initialized. Consider calling Expect");
			return field;
		}
		set;
	} = null!;

	public void Init(IExample partExampleToDelegate)
	{
		PartExample = partExampleToDelegate;
		Initialized = true;
	}

	public bool Initialized { get; private set; }

	public object Expectation => PartExample.Expectation; 
	public string ExpectationFormatted => PartExample.ExpectationFormatted;
	public object Run() => PartExample.Run();
	public object RunFormat(out string formatted) => PartExample.RunFormat(out formatted);
}


