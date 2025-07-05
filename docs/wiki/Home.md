# Advent of Code Agent

A set of [C# source generators](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview) to build an *agent* for your [Advent of Code](https://adventofcode.com/) solutions &mdash; an application that downloads inputs and submits answers.

## What it Does

1. Identifies days that have not been solved yet.
2. Checks if a day's solution meets the specified examples.
3. Downloads input, performs calculations, and submits the answers.
4. Informs you when your answer was rejected.
5. If the previous answer was given too recently, waits the necessary duration and resubmits.
6. Generates NUnit tests for the implemented solutions.
7. Caches everything so that does not hurt AoC servers much, and you don't get penalties for submitting the same incorrect answers.

## What it Does Not

1. Does not parse the input for you.
2. Does not include specialized algorithms (e.g., BFS, LCM, OCR) that are typical for AoC.

https://github.com/user-attachments/assets/fb031f1c-740d-46a6-b463-69f691a4acc0