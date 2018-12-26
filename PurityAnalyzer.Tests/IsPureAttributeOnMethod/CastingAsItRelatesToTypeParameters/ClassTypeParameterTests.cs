using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod.CastingAsItRelatesToTypeParameters
{
    [TestFixture]
    class ClassTypeParameterTests
    {

        [Test]
        public void CastingFromDerivedToBaseWhereBaseMethodDoesNotUseTAsObjectAndDerivedMethodAlsoDoesNotUseTAsObject_KeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base<T>
{
    public virtual string Method1(T input) => string.Empty;
}

public class Derived<T> : Base<T>
{
    public override string Method1(T input) => string.Empty;
}

public static class Class1
{
    [IsPure]
    public static void DoSomething()
    {
        Base<int> x = new Derived<int>();

        var result = x.Method1(1);
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CastingFromDerivedToBaseWhereBaseMethodDoesNotUseTAsObjectAndDerivedMethodUsesTAsObject_MakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base<T>
{
    public virtual string Method1(T input) => string.Empty;
}

public class Derived<T> : Base<T>
{
    public override string Method1(T input) => input.ToString();
}

public static class Class1
{
    [IsPure]
    public static void DoSomething()
    {
        Base<int> x = new Derived<int>();

        var result = x.Method1(1);
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CastingFromDerivedToBaseWhereBaseMethodUsesTAsObjectAndDerivedMethodAlsoUsesTAsObject_KeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base<T>
{
    public virtual string Method1(T input) => input.ToString();
}

public class Derived<T> : Base<T>
{
    public override string Method1(T input) => input.ToString();
}

public class SomeClass
{
}

public static class Class1
{
    [IsPure]
    public static void DoSomething()
    {
        Base<SomeClass> x = new Derived<SomeClass>();

        var result = x.Method1(new SomeClass());
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CastingFromBaseToDerivedWhereBaseMethodDoesNotUseTAsObjectAndDerivedMethodAlsoDoesNotUseTAsObject_KeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base<T>
{
    public virtual string Method1(T input) => string.Empty;
}

public class Derived<T> : Base<T>
{
    public override string Method1(T input) => string.Empty;
}

public static class Class1
{
    [IsPure]
    public static void DoSomething()
    {
        Derived<int> x = (Derived<int>) new Base<int>();
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CastingFromBaseToDerivedWhereBaseMethodDoesNotUseTAsObjectAndDerivedMethodUsesTAsObject_KeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base<T>
{
    public virtual string Method1(T input) => string.Empty;
}

public class Derived<T> : Base<T>
{
    public override string Method1(T input) => input.ToString();
}

public static class Class1
{
    [IsPure]
    public static void DoSomething()
    {
        Derived<int> x = (Derived<int>) new Base<int>();
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CastingFromBaseToDerivedWhereBaseMethodUsesTAsObjectAndDerivedMethodAlsoUsesTAsObject_KeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base<T>
{
    public virtual string Method1(T input) => input.ToString();
}

public class Derived<T> : Base<T>
{
    public override string Method1(T input) => input.ToString();
}


public static class Class1
{
    [IsPure]
    public static void DoSomething()
    {
        Derived<int> x = (Derived<int>) new Base<int>();
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CastingFromBaseToDerivedWhereBaseMethodUsesTAsObjectAndDerivedMethodDoesNotUseTAsObject_MakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base<T>
{
    public virtual string Method1(T input) => input.ToString();
}

public class Derived<T> : Base<T>
{
    public override string Method1(T input) => string.Empty;
}

public static class Class1
{
    [IsPure]
    public static void DoSomething()
    {
        Derived<int> x = (Derived<int>) new Base<int>();
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CastingFromObjectToDerivedWhereDerivedMethodDoesNotUseTAsObject_MakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base<T>
{
    public virtual string Method1(T input) => string.Empty;
}

public class Derived<T> : Base<T>
{
    private static int state = 0;

    public override string Method1(T input)
    {
        state++;
        return string.Empty;
    }
}

public static class Class1
{
    [IsPure]
    public static void DoSomething(object input)
    {
        Derived<int> x = (Derived<int>) input;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CastingFromObjectToDerivedWhereDerivedMethodUsesTAsObject_KeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base<T>
{
    public virtual string Method1(T input) => string.Empty;
}

public class Derived<T> : Base<T>
{
    private static int state = 0;

    public override string Method1(T input)
    {
        state++;
        return input.ToString();
    }
}

public static class Class1
{
    [IsPure]
    public static void DoSomething(object input)
    {
        Derived<int> x = (Derived<int>)input;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CastingFromObjectToDerivedWhereDerivedMethodDoesNotUseTAsObject_AndDerivedMethodIsSealed_KeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base<T>
{
    public virtual string Method1(T input) => string.Empty;
}

public class Derived<T> : Base<T>
{
    private static int state = 0;

    public sealed override string Method1(T input)
    {
        state++;
        return string.Empty;
    }
}

public static class Class1
{
    [IsPure]
    public static void DoSomething(object input)
    {
        Derived<int> x = (Derived<int>) input;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

    }

    //TODO: test case where derived type is non-generic
    //TODO: test type parameters of a class containing the class that has the method
    //TODO: test interfaces
}
