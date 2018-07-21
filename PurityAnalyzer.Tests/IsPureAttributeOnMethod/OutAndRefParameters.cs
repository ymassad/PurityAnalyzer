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
    public class OutAndRefParameters
    {
        [Test]
        public void MethodThatIncrementsRefIntParameterIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static void DoSomething(ref int x)
    {
        x++;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void MethodThatSetsOutIntParameterIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static void DoSomething(out int x)
    {
        x = 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void PassingFieldOfParameterAsArgumentToARefParameterMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int field;
}

public static class Module1
{

    [IsPure]
    public static void DoSomething0(Class1 class1)
    {
        DoSomething(ref class1.field);
    }

    public static void DoSomething(ref int x)
    {
        x++;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void PassingFieldOfParameterAsArgumentToAOutParameterMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int field;
}

public static class Module1
{

    [IsPure]
    public static void DoSomething0(Class1 class1)
    {
        DoSomething(out class1.field);
    }

    public static void DoSomething(out int x)
    {
        x = 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void PassingFieldOfObjectStoredInStaticFieldAsArgumentToARefParameterMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int field;
}

public static class Module1
{

    static Class1 class1 = new Class1();

    [IsPure]
    public static void DoSomething0()
    {
        DoSomething(ref class1.field);
    }

    public static void DoSomething(ref int x)
    {
        x++;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void PassingFieldOfObjectStoredInStaticFieldAsArgumentToAOutParameterMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int field;
}

public static class Module1
{
    static Class1 class1 = new Class1();

    [IsPure]
    public static void DoSomething0()
    {
        DoSomething(out class1.field);
    }

    public static void DoSomething(out int x)
    {
        x = 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void PassingStaticFieldAsArgumentToARefParameterMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    static int state = 0;

    [IsPure]
    public static void DoSomething0()
    {
        DoSomething(ref state);
    }

    public static void DoSomething(ref int x)
    {
        x++;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void PassingStaticFieldAsArgumentToAOutParameterMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    static int state = 0;

    [IsPure]
    public static void DoSomething0()
    {
        DoSomething(out state);
    }

    public static void DoSomething(out int x)
    {
        x = 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }


        [Test]
        public void PassingFieldOfNewObjectAsArgumentToARefParameterKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int field;
}

public static class Module1
{
    [IsPure]
    public static void DoSomething0()
    {
        Class1 class1 = new Class1();
        DoSomething(ref class1.field);
    }

    public static void DoSomething(ref int x)
    {
        x++;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void PassingFieldOfNewObjectAsArgumentToAOutParameterKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    public int field;
}

public static class Module1
{

    [IsPure]
    public static void DoSomething0()
    {
        Class1 class1 = new Class1();
        DoSomething(out class1.field);
    }

    public static void DoSomething(out int x)
    {
        x = 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void PassingLocalVariableAsArgumentToARefParameterKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static void DoSomething0()
    {
        int v = 0;
        DoSomething(ref v);
    }

    public static void DoSomething(ref int x)
    {
        x++;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void PassingLocalVariableAsArgumentToAOutParameterKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{

    [IsPure]
    public static void DoSomething0()
    {
        int v = 0;
        DoSomething(out v);
    }

    public static void DoSomething(out int x)
    {
        x = 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }
    }


    public class IsPureAttribute : Attribute
    {
    }

    public class Class1
    {
        public int field;
    }

    public static class Module1
    {

        [IsPure]
        public static void DoSomething0(Class1 class1)
        {
            DoSomething(out class1.field);
        }

        public static void DoSomething(out int x)
        {
            x = 1;
        }
    }
}
