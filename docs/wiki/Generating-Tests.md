# Tests Generation

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

5. Define a class decorated with the `[GenerateInputTests]` attribute and provide your answers to puzzles. This will also require manually downloading your personal inputs.

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

6. The class now becomes a test fixture verifying the solutions using inputs from the files.

