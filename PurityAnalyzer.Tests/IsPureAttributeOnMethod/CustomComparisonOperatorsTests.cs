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
    public class CustomComparisonOperatorsTests
    {
        [Test]
        public void PureCustomEqualityComparisonOperatorMethodIsConsideredPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class CustomType
{
    [IsPure]
    public static bool operator ==(CustomType c1, CustomType c2)
    {
        return true;
    }

    public static bool operator !=(CustomType c1, CustomType c2)
    {
        return true;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void ImpureCustomEqualityComparisonOperatorMethodIsConsideredImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class CustomType
{
    static int state = 0;

    [IsPure]
    public static bool operator ==(CustomType c1, CustomType c2)
    {
        state++;
        return true;
    }

    public static bool operator !=(CustomType c1, CustomType c2)
    {
        return true;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void MethodThatUsesPureCustomEqualityComparisonOperatorIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class MyClass
{
    [IsPure]
    public static bool DoSomething()
    {
        return new CustomType() == new CustomType();
    }
}

public class CustomType
{    
    public static bool operator ==(CustomType c1, CustomType c2)
    {
        return true;
    }

    public static bool operator !=(CustomType c1, CustomType c2)
    {
        return true;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void MethodThatUsesImpureCustomEqualityComparisonOperatorIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class MyClass
{
    [IsPure]
    public static bool DoSomething()
    {
        return new CustomType() == new CustomType();
    }
}

public class CustomType
{
    static int state = 0;

    public static bool operator ==(CustomType c1, CustomType c2)
    {
        state++;
        return true;
    }

    public static bool operator !=(CustomType c1, CustomType c2)
    {
        return true;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }



        [Test]
        public void PureCustomInequalityComparisonOperatorMethodIsConsideredPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class CustomType
{
    [IsPure]
    public static bool operator !=(CustomType c1, CustomType c2)
    {
        return true;
    }

    public static bool operator ==(CustomType c1, CustomType c2)
    {
        return true;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void ImpureCustomInequalityComparisonOperatorMethodIsConsideredImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class CustomType
{
    static int state = 0;

    [IsPure]
    public static bool operator !=(CustomType c1, CustomType c2)
    {
        state++;
        return true;
    }

    public static bool operator ==(CustomType c1, CustomType c2)
    {
        return true;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void MethodThatUsesPureCustomInequalityComparisonOperatorIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class MyClass
{
    [IsPure]
    public static bool DoSomething()
    {
        return new CustomType() != new CustomType();
    }
}

public class CustomType
{    
    public static bool operator !=(CustomType c1, CustomType c2)
    {
        return true;
    }

    public static bool operator ==(CustomType c1, CustomType c2)
    {
        return true;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void MethodThatUsesImpureCustomInequalityComparisonOperatorIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class MyClass
{
    [IsPure]
    public static bool DoSomething()
    {
        return new CustomType() != new CustomType();
    }
}

public class CustomType
{
    static int state = 0;

    public static bool operator !=(CustomType c1, CustomType c2)
    {
        state++;
        return true;
    }

    public static bool operator ==(CustomType c1, CustomType c2)
    {
        return true;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

    }
}
