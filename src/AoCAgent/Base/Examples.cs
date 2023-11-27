using System.ComponentModel;
using JetBrains.Annotations;

namespace mazharenko.AoCAgent.Base;

[EditorBrowsable(EditorBrowsableState.Never)]
[UsedImplicitly]
public record NamedExample(string Name, IExample<object> Example);

[EditorBrowsable(EditorBrowsableState.Never)]
[UsedImplicitly]
public interface IExample<out TRes>
{
	TRes Expectation { get; }
	string ExpectationFormatted { get; }
	TRes Run();
	TRes RunFormat(out string formatted);
}

// sadly, covariance for value types is a nonsense 
[EditorBrowsable(EditorBrowsableState.Never)]
[UsedImplicitly]
public class ExampleAdapter<TRes> : IExample<object>
	where TRes : struct
{
	private readonly IExample<TRes> example;
	public ExampleAdapter(IExample<TRes> example) => this.example = example;
	public object Expectation => example.Expectation;
	public string ExpectationFormatted => example.ExpectationFormatted;
	public object Run() => example.Run();
	public object RunFormat(out string formatted) => example.RunFormat(out formatted);
}


