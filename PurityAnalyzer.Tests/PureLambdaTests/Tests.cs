using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.PureLambdaTests
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void LambdaThatReturnsConstantIntegerIsPure()
        {
            PurityAnalyzerAnalyzer.PureLambdaMethod = ("PureLambdaClass", "Pure");

            string code = @"
using System;

public static class PureLambdaClass
{
    public static Func<T> Pure<T>(Func<T> func) => func;
}

public static class Module1
{
    public static void DoSomething()
    {
        var func1 = PureLambdaClass.Pure(() => 1);
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void LambdaThatReadsStaticFieldIsImpure()
        {
            PurityAnalyzerAnalyzer.PureLambdaMethod = ("PureLambdaClass", "Pure");

            string code = @"
using System;

public static class PureLambdaClass
{
    public static Func<T> Pure<T>(Func<T> func) => func;
}

public static class Module1
{
    public static int state = 0;

    public static void DoSomething()
    {
        var func1 = PureLambdaClass.Pure(() => state);
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void OnlyLambdasCanBePassedToPureLambdaMethods()
        {
            PurityAnalyzerAnalyzer.PureLambdaMethod = ("PureLambdaClass", "Pure");

            string code = @"
using System;

public static class PureLambdaClass
{
    public static Func<T> Pure<T>(Func<T> func) => func;
}

public static class Module1
{
    public static void DoSomething()
    {
        var func1 = PureLambdaClass.Pure(Method);
    }
    
    public static int Method() => 1;

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }



        [Test]
        public void LambdaThatModifiesIntegerDefinedInsideItIsPure()
        {
            PurityAnalyzerAnalyzer.PureLambdaMethod = ("PureLambdaClass", "Pure");

            string code = @"
using System;

public static class PureLambdaClass
{
    public static Func<T> Pure<T>(Func<T> func) => func;
}

public static class Module1
{
    public static void DoSomething()
    {
        var func1 = PureLambdaClass.Pure(() => { int a = 1; a++; return a;});
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void LambdaThatModifiesIntegerDefinedInsideTheParentMethodIsImpure()
        {
            PurityAnalyzerAnalyzer.PureLambdaMethod = ("PureLambdaClass", "Pure");

            string code = @"
using System;

public static class PureLambdaClass
{
    public static Func<T> Pure<T>(Func<T> func) => func;
}

public static class Module1
{
    public static void DoSomething()
    {
        int state = 0;

        var func1 = PureLambdaClass.Pure(() => {state++; return state;});
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void LambdaThatReadsIntegerDefinedInsideTheParentMethodIsImpure()
        {
            PurityAnalyzerAnalyzer.PureLambdaMethod = ("PureLambdaClass", "Pure");

            string code = @"
using System;

public static class PureLambdaClass
{
    public static Func<T> Pure<T>(Func<T> func) => func;
}

public static class Module1
{
    public static void DoSomething()
    {
        int state = 0;

        var func1 = PureLambdaClass.Pure(() => {return state;});
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void LambdaThatReadsItsParameterIsPure()
        {
            PurityAnalyzerAnalyzer.PureLambdaMethod = ("PureLambdaClass", "Pure");

            string code = @"
using System;

public static class PureLambdaClass
{
    public static Func<T,T> Pure<T>(Func<T,T> func) => func;
}

public static class Module1
{
    public static void DoSomething()
    {
        var func1 = PureLambdaClass.Pure((int x) => x + 1);
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void LambdaThatReadsParentMethodParameterIsImpure()
        {
            PurityAnalyzerAnalyzer.PureLambdaMethod = ("PureLambdaClass", "Pure");

            string code = @"
using System;

public static class PureLambdaClass
{
    public static Func<T,T> Pure<T>(Func<T,T> func) => func;
}

public static class Module1
{
    public static void DoSomething(int param)
    {
        var func1 = PureLambdaClass.Pure((int x) => x + param);
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void PureLambdaCanInvokeAnotherPureLambdaThatIsOutOfScope()
        {
            PurityAnalyzerAnalyzer.PureLambdaMethod = ("PureLambdaClass", "Pure");

            string code = @"
using System;

public static class PureLambdaClass
{
    public static Func<T> Pure<T>(Func<T> func) => func;
}

public static class Module1
{
    public static void DoSomething()
    {
        var func1 = PureLambdaClass.Pure(() => 1);

        var func2 = PureLambdaClass.Pure(() => func1());
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void PureLambdaCanInvokeAnotherPureLambdaThatIsOutOfScopeAfterStoringItInLocalVariable()
        {
            PurityAnalyzerAnalyzer.PureLambdaMethod = ("PureLambdaClass", "Pure");

            string code = @"
using System;

public static class PureLambdaClass
{
    public static Func<T> Pure<T>(Func<T> func) => func;
}

public static class Module1
{
    public static void DoSomething()
    {
        var func1 = PureLambdaClass.Pure(() => 1);

        var func2 = PureLambdaClass.Pure(() => { var myfunc1 = func1; return myfunc1();});
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void PureLambdaCannotModifyAnotherPureLambdaThatIsOutOfScope()
        {
            PurityAnalyzerAnalyzer.PureLambdaMethod = ("PureLambdaClass", "Pure");

            string code = @"
using System;

public static class PureLambdaClass
{
    public static Func<T> Pure<T>(Func<T> func) => func;
}

public static class Module1
{
    public static void DoSomething()
    {
        var func1 = PureLambdaClass.Pure(() => 1);

        var func2 = PureLambdaClass.Pure(() => { func1 = PureLambdaClass.Pure(() => 2); return 1;});
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void PureLambdaCanModifyAnotherPureLambdaThatIsInScope()
        {
            PurityAnalyzerAnalyzer.PureLambdaMethod = ("PureLambdaClass", "Pure");

            string code = @"
using System;

public static class PureLambdaClass
{
    public static Func<T> Pure<T>(Func<T> func) => func;
}

public static class Module1
{
    public static void DoSomething()
    {
        var func2 = PureLambdaClass.Pure(() =>
        {
            var func1 = PureLambdaClass.Pure(() => 1);
            func1 = PureLambdaClass.Pure(() => 2);
            return 1;
        });
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

    }
}
