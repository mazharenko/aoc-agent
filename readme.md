[![Nuget package](https://img.shields.io/nuget/v/mazharenko.AoCAgent.svg?logo=nuget)](https://www.nuget.org/packages/mazharenko.AoCAgent/) 
[![License: MIT](https://img.shields.io/badge/License-MIT-lightgrey.svg)](LICENSE)

[![Open in Dev Containers](https://img.shields.io/static/v1?label=Dev%20Containers&message=Open&color=yellow&logo=visualstudiocode)](https://vscode.dev/redirect?url=vscode://ms-vscode-remote.remote-containers/cloneInVolume?url=https://github.com/mazharenko/aoc-agent)

# Advent of Code Agent

A set of [C# source generators](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview) to build an *agent* for your [Advent of Code](https://adventofcode.com/) solutions &mdash; an application that downloads inputs and submits answers.

## What it Does

1. Identifies days that have not been solved yet.
2. Checks if a day's solution meets the specified examples.
3. Downloads input, performs calculations, and submits the answers.
4. Informs you when your answer was rejected.
5. If the previous answer was given too recently, waits the necessary duration and resubmits.
6. Generates NUnit tests for the implemented solutions.
7. Caches everything so that does not hurt AoC servers much and you don't get penalties for submitting the same incorrect answers.

## What is Does Not

1. Does not parse the input for you.
2. Does not include specialized algorithms (e.g., BFS, LCM, OCR) that are typical for AoC.

## Suggested Usage

1. Create an empty console project and add the Agent package to it
   
    ```sh
    dotnet new console -n aoc
    dotnet add aoc package mazharenko.AocAgent
    ```

2. Remove the default Program.cs file
   
3. Define a single year class anywhere in the project named like `YearXXXX` with the `[GenerateEntryPoint]` attribute. Don't forget the `partial` keyword.
   
    ```cs
    [GenerateEntryPoint]
    public partial class Year2022;
    ```

4. Define day classes named like `DayXX`. Don't forget the `partial` keyword.
   
    ```cs
    partial class Day01
    {
        internal partial class Part1
        {
            public string Solve(string input)
            {
                throw new NotImplementedException();
            }
        }
        internal partial class Part2
        {
            public string Solve(string input)
            {
                throw new NotImplementedException();
            }
        }
    }
    ```

5. Run it once and provide the `session` cookie value from https://adventofcode.com/

    https://github.com/mazharenko/aoc-agent/assets/5635071/e644c7b3-d458-4587-b4e5-f8bd8ff8c054

6. Give examples and implementation.
   
    ```cs
    partial class Day01
    {
        internal partial class Part1
        {
            private readonly Example example1 = new("input", "expectation");
            private readonly Example example2 = new("input", "expectation");

            public string Solve(string input)
            {
                return "expectation";
            }
        }
        internal partial class Part2 { ... }
    }
    ```

7. Run it again. It's supposed to be launched with `dotnet watch`. In this case, if any issues with the implementation are detected, it will automatically restart after code changes.

    https://github.com/mazharenko/aoc-agent/assets/5635071/d18e2b33-a1c8-4d25-9ac0-89ed28fef2e2


> [!NOTE]  
> Multiple years within a single project, strongly typed inputs and answers, opting out of entry point generation, [not providing examples](https://github.com/mazharenko/aoc-agent/wiki/Not-providing-examples) are supported as well.

## Tests Generation

1. Create a test project referencing both `AocAgent` package and the project with day implementations, as well as the `NUnit` package.
2. Opt out of agent generation setting the `false` value for a special property in the csproj file.
   
    ```xml
    <ItemGroup>
      <CompilerVisibleProperty Include="AoCAgent_GenerateAgent" />
    </ItemGroup>
    <PropertyGroup>
      <AoCAgent_GenerateAgent>false</AoCAgent_GenerateAgent>
    </PropertyGroup>
    ```

3. Define a class decorated with the `[GenerateExampleTests]` attribute.

    ```cs
    [GenerateExampleTests]
    internal partial class ExampleTests;
    ```

4. The class now becomes a test fixture collecting examples from all the Days with a test method verifying them.

5. Define a class decorated with the `[GenerateInputTests]` attribute and provide your answers to puzzles. This will also require to manually download your personal inputs.

    ```cs
    [GenerateInputTests(nameof(GetCases))]
    internal partial class InputTests
    {
        private static IEnumerable<PartInputCaseData> GetCases()
        {
            yield return new (1, 1, "56049");
        }
    }
    ```

6. The class now becomes a test fixture verifying the solutions using inputs from files.

## Repository Templates

[aoc-agent-template](https://github.com/mazharenko/aoc-agent-template) and [aoc-agent-template-multipleyears](https://github.com/mazharenko/aoc-agent-template-multipleyears) are prepared repository templates with all required references and 25 day drafts. The latter assumes that the repository will contain solutions for several years. 