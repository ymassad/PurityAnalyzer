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
    class Tests
    {

        [Test]
        public void CastingFromDerivedToBaseWhereBaseMethodDoesNotUseTAsObjectAndDerivedMethodAlsoDoesNotUseTAsObject_KeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    public virtual string Method1<T>(T input) => string.Empty;
}

public class Derived : Base
{
    public override string Method1<T>(T input) => string.Empty;
}

public static class Class1
{
    [IsPure]
    public static void DoSomething()
    {
        Base x = new Derived();

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

public class Base
{
    public virtual string Method1<T>(T input) => string.Empty;
}

public class Derived : Base
{
    public override string Method1<T>(T input) => input.ToString();
}

public static class Class1
{
    [IsPure]
    public static void DoSomething()
    {
        Base x = new Derived();

        var result = x.Method1(1);
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }


    }
}
