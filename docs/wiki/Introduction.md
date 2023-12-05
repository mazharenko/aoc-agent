This page explains the universal conventions related to creating the agent. Some other essential things are explained in other pages related to different approaches preferred by the user.

## 1. Creating a project

The project must be a console application and reference the `mazharenko.AoCAgent` package

```sh
dotnet new console -n aoc
dotnet add aoc package mazharenko.AocAgent
```

## 2. Defining days

Define day classes named like `DayXX`. Don't forget the `partial` keyword. Provide examples and implementation.
   
```cs
partial class Day01
{
    internal partial class Part1
    {
    }
    internal partial class Part2 
    {
    }
}
```

A lot of internal staff will be generated now to implement the agent's functionality. Namely, an interface, a base class and a class for examples. At this moment the complier will require to implement the `string Solve(string input)` methods from the interface.

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

## 3. Types

Strongly typed inputs and results are supported. The "implemented" `Solve` method can be changed according to the desired types.

```cs
partial class Day01
{
    internal partial class Part1
    {
        public int Solve(int[] input)
        {
            throw new NotImplementedException();
        }
    }
    internal partial class Part2 { ... }
}
```

A lot of internal staff will be regenerated now to reflect the desired types, so the compiler is going to stop complaining shortly. However, for non-string inputs it will now require to implement the `T Parse(string input)` method.

```cs
partial class Day01
{
    internal partial class Part1
    {
        public int[] Parse(string input)
        {
            throw new NotImplementedException();
        }
        public int Solve(int[] input)
        {
            throw new NotImplementedException();
        }
    }
    internal partial class Part2 { ... }
}
```

For the result's type, there still must be the way to convert it to a string. By default, its `ToString` method will be called. This can be overridden:

```cs
partial class Day01
{
    internal partial class Part1
    {
        ...
        public override string Format(int res)
        {
            return base.Format(res);
        }
        ...
    }
    ...
}

```

## 4. This is not enough

A few questions must be answered before moving on. Depending on one's preferences, further actions might differ.

1. Do you want the library to generate the Entry point for you? 

   An agent is a console application. Its entry point is supposed to call all the running logic. The entry point can be generated automatically or written manually.

2. Do you follow the repo-per-year scheme or have a single repository for all the Advent of Code events?

   How days are discovered and associated with years needs a bit more attention for the latter.


Follow the link which answers these question best.

[Entry point generation, Single year in project](Entry-point-generation,-Single-year-in-project)

[Entry point generation, Multiple years in project](Entry-point-generation,-Multiple-years-in-project)

[Manual Entry point, Single year in project](Manual-Entry-point,-Single-year-in-project)

[Manual Entry point, Multiple years in project](Manual-Entry-point,-Multiple-years-in-project)