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

        [Test]
        public void MethodThatCallsAPureExceptLocallyMethodOnNewlyCreatedObjectDirectlyIsPure()
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
        new Class1().Increment();

        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReadsAReadWriteFieldOnNewlyCreatedObjectDirectlyIsPure()
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
        return new Class1().a;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatWritesAReadWriteFieldOnNewlyCreatedObjectDirectlyIsPure()
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
        new Class1().a = 2;

        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatIncrementsViaPlusPlusAReadWriteFieldOnNewlyCreatedObjectDirectlyIsPure()
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
        new Class1().a++;

        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyMethodOnNewlyCreatedObjectViaAnotherMethodIsPure()
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
        Create().Increment();

        return 1;
    }

    public static Class1 Create() => new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReadsAReadWriteFieldOnNewlyCreatedObjectViaAnotherMethodIsPure()
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
        return Create().a;
    }

    public static Class1 Create() => new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatWritesAReadWriteFieldOnNewlyCreatedObjectViaAnotherMethodIsPure()
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
        Create().a = 2;

        return 1;
    }

    public static Class1 Create() => new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatIncrementsViaPlusPlusAReadWriteFieldOnNewlyCreatedObjectViaAnotherMethodIsPure()
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
        Create().a++;

        return 1;
    }

    public static Class1 Create() => new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyMethodOnNewlyCreatedObjectViaAnotherMethodAndAssignedToVariableIsPure()
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
        var class1 = Create();
        class1.Increment();

        return 1;
    }

    public static Class1 Create() => new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReadsAReadWriteFieldOnNewlyCreatedObjectViaAnotherMethodAndAssignedToVariableIsPure()
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
        var class1 = Create();
        return class1.a;
    }

    public static Class1 Create() => new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatWritesAReadWriteFieldOnNewlyCreatedObjectViaAnotherMethodAndAssignedToVariableIsPure()
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
        var class1 = Create();
        class1.a = 2;

        return 1;
    }

    public static Class1 Create() => new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatIncrementsViaPlusPlusAReadWriteFieldOnNewlyCreatedObjectViaAnotherMethodAndAssignedToVariableIsPure()
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
        var class1 = Create();
        class1.a++;

        return 1;
    }

    public static Class1 Create() => new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

    }
}
