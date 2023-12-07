To help the agent generators to understand to which years the days in the project belong, define a class named like `YearXXXX`.

```cs
public partial class Year2023;
```

Now pass its instance to the `Runner.Run` method in your entry point.

```cs
await new mazharenko.AoCAgent.Runner().Run(new Year2023());
```