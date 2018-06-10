using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod
{
    [TestFixture]
    public class CompiledPropertiesOnInputParameterTests
    {
        [Test]
        public void MethodThatReadsAnAutomaticReadOnlyPropertyThatDoesNotHaveTheIsPureAttributeOnParameterWhoseTypeIsCompiledIsImpure()
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
    public static string DoSomething(ImmutableDto1 input)
    {
        return input.Prop1.ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());

            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReadsAnAutomaticReadOnlyPropertyThatHasTheIsPureAttributeOnParameterWhoseTypeIsCompiledIsPure()
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
    public static string DoSomething(ImmutableDto1WithIsPureAttributeOnProperty input)
    {
        return input.Prop1.ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());

            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReadsAnAutomaticReadOnlyPropertyWhoseClassHasTheIsPureAttributeOnParameterWhoseTypeIsCompiledIsPure()
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
    public static string DoSomething(ImmutableDto1WithIsPureAttributeOnClass input)
    {
        return input.Prop1.ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());

            dignostics.Length.Should().Be(0);
        }


        [Test]
        public void MethodThatReadsAnAutomaticReadWritePropertyOnParameterWhoseTypeIsCompiledIsImpure()
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
    public static string DoSomething(MutableDto1 input)
    {
        return input.Prop1.ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());

            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatWritesAnAutomaticReadWritePropertyOnParameterWhoseTypeIsCompiledIsImpure()
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
    public static string DoSomething(MutableDto1 input)
    {
        input.Prop1 = 1;
        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());

            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReadsAndWritesAnAutomaticReadWritePropertyOnParameterWhoseTypeIsCompiledIsImpure()
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
    public static string DoSomething(MutableDto1 input)
    {
        input.Prop1 = input.Prop1 + 1;
        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());

            dignostics.Length.Should().BePositive();
        }
    }
}
