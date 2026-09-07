# PurityAnalyzer

PurityAnalyzer is a Roslyn analyzer for C# that helps developers write and maintain pure code.

A pure method should produce the same result for the same inputs and should not have externally visible side effects. Among other things, this means it should not depend on mutable global state, mutate external state, or call impure methods.

PurityAnalyzer lets you mark methods or classes as pure and reports diagnostics when their implementation violates those purity rules.

## Simple example

```csharp
[IsPure]
public static int Add(int x, int y)
{
    return x + y; // OK
}

[IsPure]
public static void Print(string text)
{
    Console.WriteLine(text); // Error: Console.WriteLine is impure
}
```
PurityAnalyzer also handles more subtle cases involving mutable objects (e.g. `List<T>`), casting, locally-created objects, and different levels of purity.

For more information see the Writing Pure Code in C# article here: https://www.dotnetcurry.com/csharp/1464/pure-code-csharp
