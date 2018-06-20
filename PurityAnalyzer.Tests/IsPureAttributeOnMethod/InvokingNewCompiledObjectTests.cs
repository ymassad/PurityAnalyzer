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

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().Be(0);
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

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().Be(0);
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

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().Be(0);
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

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().Be(0);
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

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().Be(0);
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

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().Be(0);
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

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().Be(0);
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

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().Be(0);
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

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().Be(0);
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

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().Be(0);
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

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().Be(0);
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

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().Be(0);
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

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().Be(0);
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

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().Be(0);
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

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().Be(0);
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

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().Be(0);
        }

    }
}
