using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.PureAttribute
{
    [TestFixture]
    public class PureIsSameAsIsPure
    {
        [Test]
        public void CaseWhereMembersArePure()
        {
            string code = @"
using System;

public class PureAttribute : Attribute
{
}

[Pure]
public class Class1
{

    public int Prop1
    {
        get
        {
            return 1;
        }
    }

    public int DoSomething(int a)
    {
        return a + 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void PureOnClassRequiresInstanceConstructorToBePure()
        {
            string code = @"
using System;

public class PureAttribute : Attribute
{
}

[Pure]
public class Class1
{
    public Class1() => AnotherClass.state = 1;
}

public static class AnotherClass
{
    public static int state;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void PureAttributeOnClassRequiresInstanceConstructorToBePure()
        {
            string code = @"
using System;

public class PureAttribute : Attribute
{
}

[PureAttribute]
public class Class1
{
    public Class1() => AnotherClass.state = 1;
}

public static class AnotherClass
{
    public static int state;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }
    }


}