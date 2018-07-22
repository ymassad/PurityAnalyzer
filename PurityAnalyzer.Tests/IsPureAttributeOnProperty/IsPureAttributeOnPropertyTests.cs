using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnProperty
{
    [TestFixture]
    public class IsPureAttributeOnPropertyTests
    {
        [Test]
        public void TestIsPureAttributeOnStaticAutomaticReadonlyProperty()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    static Class1()
    {
        Prop1 = 1;
    }
    
    [IsPure]
    public static int Prop1 {get;}
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void TestIsPureAttributeOnStaticPropertyThatIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    static int a;

    [IsPure]
    public static int Prop1
    {
        get
        {
            return a++;
        }
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }


        [Test]
        public void TestIsPureAttributeOnStaticPropertyThatIsImpureAndThatIsExpressionBodied()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    static int a;

    [IsPure]
    public static int Prop1 => a++;
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void TestIsPureAttributeOnStaticPropertyThatIsImpureAndThatIsExpressionBodiedGet()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Class1
{
    static int a;

    [IsPure]
    public static int Prop1
    {
        get => a++;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void PropertyWhoseGetterInvokesAPureExceptLocallyPropertyGetterAndSetterGetOnNewlyCreatedCompiledObjectIsPure()
        {
            string code = @"
using System;
using PurityAnalyzer.Tests.CompiledCsharpLib;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static int Prop
    {
        get
        {
            var instance = new MutableClassWithPurePropertiesExceptLocally();

            return instance.PureExceptLocallyPropertyGetterAndSetter;
        }
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void ExpressionBodiedPropertyThatInvokesAPureExceptLocallyPropertyGetterAndSetterGetOnNewlyCreatedCompiledObjectIsPure()
        {
            string code = @"
using System;
using PurityAnalyzer.Tests.CompiledCsharpLib;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static int Prop => new MutableClassWithPurePropertiesExceptLocally().PureExceptLocallyPropertyGetterAndSetter;
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().Be(0);
        }


        [Test]
        public void CastFromNewObjectAndPassResultToPureExceptReadLocallyMethodViaTwoVariablesAndOneIsInitializedInDifferentStatement_KeepsMethodPure()
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
    public static int DoSomething
    {
        get
        {
            Class1 class1 = new Class1();

            Base x;

            x = new Derived();

            Base y = x;

            class1.PureExceptReadLocally(y);

            return 1;
        }
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
        public void CastFromNewObjectAndPassResultToPureExceptLocallyMethodViaTwoVariablesAndOneIsInitializedInDifferentStatement_MakesMethodImpure()
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
    public static int DoSomething
    {
        get
        {
            Class1 class1 = new Class1();

            Base x;

            x = new Derived();
            
            Base y = x;

            class1.PureExceptLocally(y);

            return 1;
        }
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
    }
}
