using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod
{
    [TestFixture]
    public class InvokingNewObjectTests
    {
        [Test]
        public void MethodThatCallsAPureExceptLocallyMethodOnNewlyCreatedObjectIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;

    public void Increment() => a++;
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        var instance = new Class1();

        instance.Increment();

        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReadsAReadWriteFieldOnNewlyCreatedObjectIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        var instance = new Class1();

        return instance.a;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatWritesAReadWriteFieldOnNewlyCreatedObjectIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        var instance = new Class1();

        instance.a = 2;

        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatIncrementsViaPlusPlusAReadWriteFieldOnNewlyCreatedObjectIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int a;
}

public static class Module1
{
    
    [IsPure]
    public static int DoSomething()
    {
        var instance = new Class1();

        instance.a++;

        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }
    }
}
