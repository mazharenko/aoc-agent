Sometimes an input is relatively small but way too tough to parse automatically. You can decorate a day or a part with the `[ManualInput]` attribute and not waste precious time on implementing the `Parse` method. Instead, the internal types will be generated in a way so that the compiler will require to implement the `ManualInput` method.

Compare:

```cs
internal partial class Part1
{
    // Example's input is string
    private readonly Example example = new(
        """
        Time:      7  15   30
        Distance:  9  40  200
        """, 288);

    public (long, long)[] Parse(string input) => ... ;
    // Solve's input as the same as Parse's output
    public int Solve((long, long)[] input) => ... ;
}
```


```cs
[ManualInput]
internal partial class Part1
{
    // Example's input is the same as Solve's input
    private readonly Example example = new(
        new []
          {
            (7, 9),
            (15, 40),
            (30, 200)
          }, 288);
    // ManualInput's output is the same as Solve's input
    // return your real input here
    public (long, long)[] ManualInput() => new[]
    {
        (41, 249),
        (77, 1362),
        (70, 1127),
        (96, 1011)
    };
    // No Parse method
    public int Solve((long, long)[] input) => ...;
   
}
```


Tests generation is still supported, but `GenerateInputTests`-decorated test fixture requires empty input text files. 