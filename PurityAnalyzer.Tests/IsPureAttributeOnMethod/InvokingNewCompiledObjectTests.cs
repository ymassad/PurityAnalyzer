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
    public class InvokingNewCompiledObjectTests
    {
        [Test]
        public void MethodThatCallsAPureExceptLocallyMethodOnNewlyCreatedCompiledObjectIsPure()
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
    public static int DoSomething()
    {
        var instance = new MutableClassWithPureMethodsExceptLocally();

        instance.Increment();

        return 1;
    }
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatReadsAReadWriteFieldOnNewlyCreatedCompiledObjectIsPure()
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
    public static int DoSomething()
    {
        var instance = new MutableClassWithPureMethodsExceptLocally();

        return instance.state;
    }
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatWritesAReadWriteFieldOnNewlyCreatedCompiledObjectIsPure()
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
    public static int DoSomething()
    {
        var instance = new MutableClassWithPureMethodsExceptLocally();

        instance.state = 2;

        return 1;
    }
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatIncrementsViaPlusPlusAReadWriteFieldOnNewlyCreatedCompiledObjectIsPure()
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
    public static int DoSomething()
    {
        var instance = new MutableClassWithPureMethodsExceptLocally();

        instance.state++;

        return 1;
    }
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyMethodOnNewlyCreatedCompiledObjectDirectlyIsPure()
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
    public static int DoSomething()
    {
        new MutableClassWithPureMethodsExceptLocally().Increment();

        return 1;
    }
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatReadsAReadWriteFieldOnNewlyCreatedCompiledObjectDirectlyIsPure()
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
    public static int DoSomething()
    {
        return new MutableClassWithPureMethodsExceptLocally().state;
    }
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatWritesAReadWriteFieldOnNewlyCreatedCompiledObjectDirectlyIsPure()
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
    public static int DoSomething()
    {
        new MutableClassWithPureMethodsExceptLocally().state = 2;

        return 1;
    }
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatIncrementsViaPlusPlusAReadWriteFieldOnNewlyCreatedCompiledObjectDirectlyIsPure()
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
    public static int DoSomething()
    {
        new MutableClassWithPureMethodsExceptLocally().state++;

        return 1;
    }
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyMethodOnNewlyCreatedCompiledObjectViaAnotherMethodIsPure()
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
    public static int DoSomething()
    {
        Create().Increment();

        return 1;
    }

    public static MutableClassWithPureMethodsExceptLocally Create() => new MutableClassWithPureMethodsExceptLocally();
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatReadsAReadWriteFieldOnNewlyCreatedCompiledObjectViaAnotherMethodIsPure()
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
    public static int DoSomething()
    {
        return Create().state;
    }

    public static MutableClassWithPureMethodsExceptLocally Create() => new MutableClassWithPureMethodsExceptLocally();
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatWritesAReadWriteFieldOnNewlyCreatedCompiledObjectViaAnotherMethodIsPure()
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
    public static int DoSomething()
    {
        Create().state = 2;

        return 1;
    }

    public static MutableClassWithPureMethodsExceptLocally Create() => new MutableClassWithPureMethodsExceptLocally();
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatIncrementsViaPlusPlusAReadWriteFieldOnNewlyCreatedCompiledObjectViaAnotherMethodIsPure()
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
    public static int DoSomething()
    {
        Create().state++;

        return 1;
    }

    public static MutableClassWithPureMethodsExceptLocally Create() => new MutableClassWithPureMethodsExceptLocally();
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyMethodOnNewlyCreatedCompiledObjectViaAnotherMethodAndAssignedToVariableIsPure()
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
    public static int DoSomething()
    {
        var class1 = Create();
        class1.Increment();

        return 1;
    }

    public static MutableClassWithPureMethodsExceptLocally Create() => new MutableClassWithPureMethodsExceptLocally();
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatReadsAReadWriteFieldOnNewlyCreatedCompiledObjectViaAnotherMethodAndAssignedToVariableIsPure()
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
    public static int DoSomething()
    {
        var class1 = Create();
        return class1.state;
    }

    public static MutableClassWithPureMethodsExceptLocally Create() => new MutableClassWithPureMethodsExceptLocally();
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatWritesAReadWriteFieldOnNewlyCreatedCompiledObjectViaAnotherMethodAndAssignedToVariableIsPure()
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
    public static int DoSomething()
    {
        var class1 = Create();
        class1.state = 2;

        return 1;
    }

    public static MutableClassWithPureMethodsExceptLocally Create() => new MutableClassWithPureMethodsExceptLocally();
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatIncrementsViaPlusPlusAReadWriteFieldOnNewlyCreatedCompiledObjectViaAnotherMethodAndAssignedToVariableIsPure()
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
    public static int DoSomething()
    {
        var class1 = Create();
        class1.state++;

        return 1;
    }

    public static MutableClassWithPureMethodsExceptLocally Create() => new MutableClassWithPureMethodsExceptLocally();
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatInvokesGetterOfAPureExceptLocallyPropertyOnNewlyCreatedCompiledObjectIsPure()
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
    public static int DoSomething()
    {
        var instance = new MutableClassWithPurePropertiesExceptLocally();

        return instance.PureExceptLocallyProperty;
    }
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatInvokesGetterOfAPureExceptLocallyPropertyOnNewlyCreatedCompiledObjectDirectlyIsPure()
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
    public static int DoSomething()
    {
        return new MutableClassWithPurePropertiesExceptLocally().PureExceptLocallyProperty;
    }
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatInvokesGetterOfAPureExceptLocallyPropertyOnNewlyCreatedCompiledObjectViaAnotherMethodIsPure()
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
    public static int DoSomething()
    {
        return Create().PureExceptLocallyProperty;
    }

    public static MutableClassWithPurePropertiesExceptLocally Create() => new MutableClassWithPurePropertiesExceptLocally();
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatInvokesGetterOfAPureExceptLocallyPropertyOnNewlyCreatedCompiledObjectViaAnotherMethodAndAssignedToVariableIsPure()
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
    public static int DoSomething()
    {
        var instance = Create();
        return instance.PureExceptLocallyProperty;
    }

    public static MutableClassWithPurePropertiesExceptLocally Create() => new MutableClassWithPurePropertiesExceptLocally();
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatInvokesSetterOfAPureExceptLocallyPropertyOnNewlyCreatedCompiledObjectIsPure()
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
    public static int DoSomething()
    {
        var instance = new MutableClassWithPurePropertiesExceptLocally();

        instance.PureExceptLocallyProperty = 2;

        return 1;
    }
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatInvokesSetterOfAPureExceptLocallyPropertyOnNewlyCreatedCompiledObjectDirectlyIsPure()
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
    public static int DoSomething()
    {
        new MutableClassWithPurePropertiesExceptLocally().PureExceptLocallyProperty = 2;

        return 1;
    }
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatInvokesSetterOfAPureExceptLocallyPropertyOnNewlyCreatedCompiledObjectViaAnotherMethodIsPure()
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
    public static int DoSomething()
    {
        Create().PureExceptLocallyProperty = 2;

        return 1;
    }

    public static MutableClassWithPurePropertiesExceptLocally Create() => new MutableClassWithPurePropertiesExceptLocally();
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatInvokesSetterOfAPureExceptLocallyPropertyOnNewlyCreatedCompiledObjectViaAnotherMethodAndAssignedToVariableIsPure()
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
    public static int DoSomething()
    {
        var instance = Create();
        instance.PureExceptLocallyProperty = 2;

        return 1;
    }

    public static MutableClassWithPurePropertiesExceptLocally Create() => new MutableClassWithPurePropertiesExceptLocally();
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatInvokesAPureExceptLocallyPropertyGetterAndSetterGetOnNewlyCreatedCompiledObjectIsPure()
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
    public static int DoSomething()
    {
        var instance = new MutableClassWithPurePropertiesExceptLocally();

        return instance.PureExceptLocallyPropertyGetterAndSetter;
    }
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void ExpressionBodiedMethodThatInvokesAPureExceptLocallyPropertyGetterAndSetterGetOnNewlyCreatedCompiledObjectIsPure()
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
    public static int DoSomething() => new MutableClassWithPurePropertiesExceptLocally().PureExceptLocallyPropertyGetterAndSetter;
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }


        [Test]
        public void MethodThatInvokesAPureExceptLocallyPropertyGetterAndSetterGetOnNewlyCreatedCompiledObjectDirectlyIsPure()
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
    public static int DoSomething()
    {
        return new MutableClassWithPurePropertiesExceptLocally().PureExceptLocallyPropertyGetterAndSetter;
    }
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatInvokesAPureExceptLocallyPropertyGetterAndSetterGetOnNewlyCreatedCompiledObjectViaAnotherMethodIsPure()
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
    public static int DoSomething()
    {
        return Create().PureExceptLocallyPropertyGetterAndSetter;
    }

    public static MutableClassWithPurePropertiesExceptLocally Create() => new MutableClassWithPurePropertiesExceptLocally();
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatInvokesAPureExceptLocallyPropertyGetterAndSetterGetOnNewlyCreatedCompiledObjectViaAnotherMethodAndAssignedToVariableIsPure()
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
    public static int DoSomething()
    {
        var instance = Create();
        return instance.PureExceptLocallyPropertyGetterAndSetter;
    }

    public static MutableClassWithPurePropertiesExceptLocally Create() => new MutableClassWithPurePropertiesExceptLocally();
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatInvokesAPureExceptLocallyPropertyGetterAndSetterSetOnNewlyCreatedCompiledObjectIsPure()
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
    public static int DoSomething()
    {
        var instance = new MutableClassWithPurePropertiesExceptLocally();

        instance.PureExceptLocallyPropertyGetterAndSetter = 2;

        return 1;
    }
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatInvokesAPureExceptLocallyPropertyGetterAndSetterSetOnNewlyCreatedCompiledObjectDirectlyIsPure()
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
    public static int DoSomething()
    {
        new MutableClassWithPurePropertiesExceptLocally().PureExceptLocallyPropertyGetterAndSetter = 2;

        return 1;
    }
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatInvokesAPureExceptLocallyPropertyGetterAndSetterSetOnNewlyCreatedCompiledObjectViaAnotherMethodIsPure()
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
    public static int DoSomething()
    {
        Create().PureExceptLocallyPropertyGetterAndSetter = 2;

        return 1;
    }

    public static MutableClassWithPurePropertiesExceptLocally Create() => new MutableClassWithPurePropertiesExceptLocally();
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatInvokesAPureExceptLocallyPropertyGetterAndSetterSetOnNewlyCreatedCompiledObjectViaAnotherMethodAndAssignedToVariableIsPure()
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
    public static int DoSomething()
    {
        var instance = Create();
        instance.PureExceptLocallyPropertyGetterAndSetter = 2;

        return 1;
    }

    public static MutableClassWithPurePropertiesExceptLocally Create() => new MutableClassWithPurePropertiesExceptLocally();
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }


        [Test]
        public void MethodThatCallsAPureExceptLocallyMethodOnCompiledObjectObtainedByInvokingACompiledInterfaceMethodMarkedWithReturnsNewObjectAttributeIsPure()
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
    public static int DoSomething(IFactoryThatReturnsNewObject factory)
    {
        factory.Create().Increment();

        return 1;
    }
}";

            Utilities.AssertPure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }

        [Test]
        public void MethodThatCallsAPureExceptLocallyMethodOnCompiledObjectObtainedByInvokingACompiledInterfaceMethodNotMarkedWithReturnsNewObjectAttributeIsImpure()
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
    public static int DoSomething(IFactoryThatDoesNotReturnNewObject factory)
    {
        factory.Create().Increment();

        return 1;
    }
}";

            Utilities.AssertImpure(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
        }



    }
}
