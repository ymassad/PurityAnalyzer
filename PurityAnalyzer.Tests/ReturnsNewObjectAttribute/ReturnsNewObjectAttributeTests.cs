using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.ReturnsNewObjectAttribute
{
    [TestFixture]
    public class ReturnsNewObjectAttributeTests
    {
        [Test]
        public void MethodWithTheReturnsNewObjectAttributeCannotBeAppliedOnMethodsThatReturnValueTypes()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public static class Module1
{
    [ReturnsNewObject]
    public static int DoSomething()
    {
        return 1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReturnsParameterDoesNotReturnNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public class Class1
{
}

public static class Module1
{
    [ReturnsNewObject]
    public static Class1 DoSomething(Class1 class1)
    {
        return class1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReturnsFieldDoesNotReturnNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public class Class1
{
}

public static class Module1
{
    static Class1 class1;

    [ReturnsNewObject]
    public static Class1 DoSomething()
    {
        return class1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReturnsNewObjectDirectlyReturnsNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public class Class1
{
}

public static class Module1
{
    [ReturnsNewObject]
    public static Class1 DoSomething()
    {
        return new Class1();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReturnsNewObjectStoredInVariableReturnsNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public class Class1
{
}

public static class Module1
{
    [ReturnsNewObject]
    public static Class1 DoSomething()
    {
        var class1 = new Class1();
        return class1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReturnsTheResultOfCallingAMethodThatDoesNotReturnANewObjectDoesNotReturnNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public class Class1
{
}

public static class Module1
{
    [ReturnsNewObject]
    public static Class1 DoSomething()
    {
        return DoSomething2();
    }

    static Class1 class1;

    public static Class1 DoSomething2() => class1;
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void ExpressionBodiedMethodThatReturnsTheResultOfCallingAMethodThatDoesNotReturnANewObjectDoesNotReturnNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public class Class1
{
}

public static class Module1
{
    [ReturnsNewObject]
    public static Class1 DoSomething() => DoSomething2();

    static Class1 class1;

    public static Class1 DoSomething2() => class1;
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }


        [Test]
        public void MethodThatReturnsTheResultOfCallingAMethodThatReturnsANewObjectReturnsNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public class Class1
{
}

public static class Module1
{
    [ReturnsNewObject]
    public static Class1 DoSomething()
    {
        return DoSomething2();
    }

    public static Class1 DoSomething2() => new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void ExpressionBodiedMethodThatReturnsTheResultOfCallingAMethodThatReturnsANewObjectReturnsNewObject()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public class Class1
{
}

public static class Module1
{
    [ReturnsNewObject]
    public static Class1 DoSomething() => DoSomething2();

    public static Class1 DoSomething2() => new Class1();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReturnsTheResultOfCallingACompiledMethodThatReturnsANewObjectReturnsNewObject()
        {
            string code = @"
using System;
using PurityAnalyzer.Tests.CompiledCsharpLib;

public class ReturnsNewObjectAttribute : Attribute
{
}

public static class Module1
{
    [ReturnsNewObject]
    public static MutableDto1 DoSomething()
    {
        return StaticClass.CreateNew();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReturnsTheResultOfCallingACompiledMethodThatDoesNotReturnANewObjectDoesNotReturnNewObject()
        {
            string code = @"
using System;
using PurityAnalyzer.Tests.CompiledCsharpLib;

public class ReturnsNewObjectAttribute : Attribute
{
}

public static class Module1
{
    [ReturnsNewObject]
    public static MutableDto1 DoSomething()
    {
        return StaticClass.ReturnExisting();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void ReturnNewObjectAttributeCanBeAppliedOnInterfaceMethods()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public class Class1
{
}

public interface IInterface
{
    [ReturnsNewObject]
    Class1 DoSomething(Class1 class1);
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void ReturnNewObjectAttributeCanBeAppliedOnAbstractMethods()
        {
            string code = @"
using System;

public class ReturnsNewObjectAttribute : Attribute
{
}

public class Class1
{
}

public abstract class AbstractClass
{
    [ReturnsNewObject]
    public abstract Class1 DoSomething(Class1 class1);
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

    }
}
