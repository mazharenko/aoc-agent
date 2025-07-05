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

To describe which year should be run by the agent, mention this year again anywhere in the project with the `[GenerateEntryPoint]` attribute


```cs
[GenerateEntryPoint]
public partial class Year2023;
```

In addition, remove the default Program.cs file.

Now the agent can be launched, because the entry point will be generated.