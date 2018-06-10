using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod
{
    [TestFixture]
    public class FieldsOnInputParameterTests
    {
        [Test]
        public void MethodThatReadsAReadOnlyFieldOnParameterWhoseTypeIsDefinedInCodeIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public readonly int Field1 = 5;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(Dto1 input)
    {
        
        return input.Field1.ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }

        //Even if input is mutable, reading such mutable input is a pure operation
        [Test]
        public void MethodThatReadsAReadWriteFieldOnParameterWhoseTypeIsDefinedInCodeIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public int Field1 = 5;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(Dto1 input)
    {
        
        return input.Field1.ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatWritesAReadWriteFieldOnParameterWhoseTypeIsDefinedInCodeIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public int Field1 = 5;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(Dto1 input)
    {
        input.Field1 = 6;
        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatIncrementsAReadWriteFieldOnParameterWhoseTypeIsDefinedInCodeIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public int Field1 = 5;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(Dto1 input)
    {
        input.Field1 = input.Field1 + 1;
        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatIncrementsViaPlusPlusAReadWriteFieldOnParameterWhoseTypeIsDefinedInCodeIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public int Field1 = 5;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(Dto1 input)
    {
        input.Field1++;
        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatIncrementsViaPlusEqualsAReadWriteFieldOnParameterWhoseTypeIsDefinedInCodeIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public int Field1 = 5;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(Dto1 input)
    {
        input.Field1 += 1;
        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
        }
    }
}
