To generate the Entry Point in a project dedicated to a single year, define a class named like `YearXXXX` with the `[GenerateEntryPoint]` attribute. Don't forget the `partial` keyword.

```cs
[GenerateEntryPoint]
public partial class Year2023;
```

In addition, remove the default Program.cs file.

Now the agent can be launched, because the entry point will be generated.