using System;
using JetBrains.Annotations;

namespace mazharenko.AoCAgent.Generator;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
[PublicAPI]
public class GenerateInputTestsAttribute : Attribute
{
	public int? Year { get; }
	public Type SourceType { get; }
	public string SourceName { get; }

	public GenerateInputTestsAttribute(Type sourceType)
	{
		SourceType = sourceType;
	}

	public GenerateInputTestsAttribute(int year, Type sourceType)
	{
		Year = year;
		SourceType = sourceType;
	}

	public GenerateInputTestsAttribute(string sourceName)
	{
		SourceName = sourceName;
	}

	public GenerateInputTestsAttribute(int year, string sourceName)
	{
		Year = year;
		SourceName = sourceName;
	}

	public GenerateInputTestsAttribute(Type sourceType, string sourceName)
	{
		SourceType = sourceType;
		SourceName = sourceName;
	}

	public GenerateInputTestsAttribute(int year, Type sourceType, string sourceName)
	{
		Year = year;
		SourceType = sourceType;
		SourceName = sourceName;
	}
}