using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod.OverriddenMethods
{
    [TestFixture]
    public class CastingAndMethodsHaveDifferentPurityTypes
    {
        [Test]
        public void UpCastingIsAllowedWhereTargetMethodIsPureExceptLocallyAndSourceMethodIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    int state = 0;
    public virtual int Method() => state++;
}

public class Derived : Base
{
    public override int Method() => 1;
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        Base x = new Derived();

        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void UpCastingIsNotAllowedWhereTargetMethodIsPureAndSourceMethodIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    public virtual int Method() => 1;
}

public class Derived : Base
{
    int state = 0;
    public override int Method() => state++;
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        Base x = new Derived();

        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

    }
}
