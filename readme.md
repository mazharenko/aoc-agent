[![Nuget package](https://img.shields.io/nuget/v/mazharenko.AoCAgent.svg?logo=nuget)](https://www.nuget.org/packages/mazharenko.AoCAgent/) 
[![License: MIT](https://img.shields.io/badge/License-MIT-lightgrey.svg)](LICENSE)
[![Wiki](https://img.shields.io/badge/wiki-documentation-forestgreen)](https://github.com/mazharenko/aoc-agent/wiki)

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

## Quick start

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

4. Define day classes named like `DayXX`. Don't forget the `partial` keyword. Provide examples and implementation.
   
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

5. When run for the first time, it requests the `session` cookie value from https://adventofcode.com/

    https://github.com/mazharenko/aoc-agent/assets/5635071/e644c7b3-d458-4587-b4e5-f8bd8ff8c054

6. Run the program. If the implementation is correct, you will earn an Advent of Code star.

    It works best with `dotnet watch`. With it, if any issues with the implementation are detected, it will automatically restart after code changes.

    https://github.com/user-attachments/assets/fb031f1c-740d-46a6-b463-69f691a4acc0

> [!NOTE]  
> More info is provided in the [docs](https://github.com/mazharenko/aoc-agent/wiki)

## Repository Templates

[aoc-agent-template](https://github.com/mazharenko/aoc-agent-template) and [aoc-agent-template-multipleyears](https://github.com/mazharenko/aoc-agent-template-multipleyears) are prepared repository templates with all required references and 25 day drafts. The latter assumes that the repository will contain solutions for several years. 
