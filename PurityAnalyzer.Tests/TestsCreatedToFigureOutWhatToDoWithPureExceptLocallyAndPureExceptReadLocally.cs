using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests
{
    [TestFixture]
    public class TestsCreatedToFigureOutWhatToDoWithPureExceptLocallyAndPureExceptReadLocally
    {
        [Test]
        public void TryingToBreakPureExceptLocally_Test1()
        {
            string code = $@"
using System;

public class IsPureAttribute : Attribute {{}}

public class A
{{
    private int state;

    public void Increment() => state++;
    public int Read() => state;
}}

public static class B
{{
    public static readonly A A = new A();
}}

public class C
{{
    private readonly A a;

    public C()
    {{
        a = B.A;
    }}

    public void Increment() => a.Increment();
    public int Read() => a.Read();
}}

public static class Module1
{{
    [IsPure]
    public static void DoSomething()
    {{
        var c2 = new C();

        c2.Increment();
    }}
}}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive("DoSomething will cause global state to be mutated");
        }
    }
}