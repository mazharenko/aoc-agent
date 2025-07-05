To help the agent generators to understand to which years the days in the project belong, put them into corresponding namespaces.

```cs
namespace Year2022
{
    partial class Day01 { ... }
}

namespace aoc.Year2023.day01
{
    partial class Day01 { ... }
}
```

The rule is simple: any segment in a namespace identifier must fit the `YearXXXX` pattern.

For each year the generator now will generate corresponding classes. Pass the desired one to the `Runner.Run` method in your entry point.

```cs
await new mazharenko.AoCAgent.Runner().Run(new Year2023());
```