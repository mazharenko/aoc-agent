namespace mazharenko.AoCAgent.Generator.Sources;

internal readonly record struct YearClass(
	int Num,
	string Name,
	string? Namespace
)
{
	public string FullName { get; } = Namespace is null ? Name : $"{Namespace}.{Name}";
};