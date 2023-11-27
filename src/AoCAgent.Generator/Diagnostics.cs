using Microsoft.CodeAnalysis;

namespace mazharenko.AoCAgent.Generator;

public static class Diagnostics
{
	public static readonly DiagnosticDescriptor NoTestFrameworkReference = new(
		"AOC0001",
		"Test framework is not referenced",
		"Tests generation needs NUnit. Consider referencing the NUnit package.",
		"Usage",
		DiagnosticSeverity.Error,
		true
	);
	public static readonly DiagnosticDescriptor MoreThatOneExplicitYear = new(
		"AOC0002",
		"More than one explicit year marker found",
		"More than one explicit year marker found. Either define no explicit year marker" +
		" (and put days into appropriate namespaces) or define exactly one of them.",
		"Usage",
		DiagnosticSeverity.Error,
		true
	);
	public static readonly DiagnosticDescriptor NoExplicitYear = new(
		"AOC0003",
		"No explicit year marker found",
		"No explicit year marker found. Either define no explicit year marker" +
		" (and put days into appropriate namespaces) or define exactly one of them.",
		"Usage",
		DiagnosticSeverity.Error,
		true
	);
}