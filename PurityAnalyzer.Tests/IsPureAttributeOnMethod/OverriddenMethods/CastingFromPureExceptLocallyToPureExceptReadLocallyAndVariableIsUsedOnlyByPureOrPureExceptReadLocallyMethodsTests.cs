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
    public class CastingFromPureExceptLocallyToPureExceptReadLocallyAndVariableIsUsedOnlyByPureOrPureExceptReadLocallyMethodsTests
    {
        [Test]
        public void CastFromParameterAndPassResultToPureMethod_MakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    int state = 0; public virtual int Method() => state;
}

public class Derived : Base
{
    int state = 0;
    public override int Method() => state++;
}

public static class Module1
{
    [IsPure]
    public static int DoSomething(Derived source)
    {
        Base x = source;

        PureMethod(x);

        return 1;
    }

    public static void PureMethod(Base x)
    {
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }


        [Test]
        public void CastFromParameterAndPassResultToPureMethodDirectly_MakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    int state = 0; public virtual int Method() => state;
}

public class Derived : Base
{
    int state = 0;
    public override int Method() => state++;
}

public static class Module1
{
    [IsPure]
    public static int DoSomething(Derived source)
    {
        PureMethod(source);

        return 1;
    }

    public static void PureMethod(Base x)
    {
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CastFromParameterAndPassResultToPureExceptReadLocallyMethod_MakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    int state = 0; public virtual int Method() => state;
}

public class Derived : Base
{
    int state = 0;
    public override int Method() => state++;
}

public static class Module1
{
    [IsPure]
    public static int DoSomething(Derived source, Class1 class1)
    {
        Base x = source;

        class1.PureExceptReadLocally(x);

        return 1;
    }

}

public class Class1
{
    int state = 0;

    public int PureExceptReadLocally(Base x)
    {
        return state;
    }
}

";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CastFromParameterAndPassResultToPureExceptLocallyMethod_MakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    int state = 0; public virtual int Method() => state;
}

public class Derived : Base
{
    int state = 0;
    public override int Method() => state++;
}

public static class Module1
{
    [IsPure]
    public static int DoSomething(Derived source)
    {
        Base x = source;

        Class1 class1 = new Class1();

        class1.PureExceptLocally(x);

        return 1;
    }

}

public class Class1
{
    int state = 0;

    public int PureExceptLocally(Base x)
    {
        return state++;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CastFromParameterAndReturnResult_MakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    int state = 0; public virtual int Method() => state;
}

public class Derived : Base
{
    int state = 0;
    public override int Method() => state++;
}

public static class Module1
{
    [IsPure]
    public static Base DoSomething(Derived source)
    {
        Base x = source;

        return x;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }














        [Test]
        public void CastFromNewObjectAndDontUseResult_KeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    int state = 0; public virtual int Method() => state;
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
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CastFromNewObjectAndPassResultToPureMethod_KeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    int state = 0; public virtual int Method() => state;
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

        PureMethod(x);

        return 1;
    }

    public static void PureMethod(Base x)
    {
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }


        [Test]
        public void CastFromNewObjectAndPassResultToPureMethodDirectly_KeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    int state = 0; public virtual int Method() => state;
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
        PureMethod(new Derived());

        return 1;
    }

    public static void PureMethod(Base x)
    {
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CastFromNewObjectAndPassResultToPureExceptReadLocallyMethod_KeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    int state = 0; public virtual int Method() => state;
}

public class Derived : Base
{
    int state = 0;
    public override int Method() => state++;
}

public static class Module1
{
    [IsPure]
    public static int DoSomething(Class1 class1)
    {
        Base x = new Derived();

        class1.PureExceptReadLocally(x);

        return 1;
    }

}

public class Class1
{
    int state = 0;

    public int PureExceptReadLocally(Base x)
    {
        return state;
    }
}

";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        //A pure except locally method could decide to store the parameter inside a field
        [Test]
        public void CastFromNewObjectAndPassResultToPureExceptLocallyMethod_MakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    int state = 0; public virtual int Method() => state;
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

        Class1 class1 = new Class1();

        class1.PureExceptLocally(x);

        return 1;
    }

}

public class Class1
{
    int state = 0;

    public int PureExceptLocally(Base x)
    {
        return state++;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CastFromNewObjectAndReturnResult_MakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    int state = 0; public virtual int Method() => state;
}

public class Derived : Base
{
    int state = 0;
    public override int Method() => state++;
}

public static class Module1
{
    [IsPure]
    public static Base DoSomething()
    {
        Base x = new Derived();

        return x;
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

    }
}
