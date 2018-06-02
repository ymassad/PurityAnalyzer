using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests
{
    [TestFixture]
    public class PropertiesOnInputParameterTests
    {
        [Test]
        public void MethodThatReadsAnAutomaticReadOnlyPropertyOnParameterWhoseTypeIsDefinedInCodeIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public int Prop1 {get;} = 5;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(Dto1 input)
    {
        
        return input.Prop1.ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }


        [Test]
        public void MethodThatReadsAnNonAutomaticReadOnlyPropertyThatModifiesStateOnParameterWhoseTypeIsDefinedInCodeIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    int state = 0;

    public int Prop1
    {
        get
        {
            state++;
            return 1;
        }
    }
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(Dto1 input)
    {
        
        return input.Prop1.ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatReadsAnNonAutomaticReadOnlyPropertyWhoseGetterIsPureOnParameterWhoseTypeIsDefinedInCodeIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    int state = 0;

    public int Prop1
    {
        get
        {
            return 1;
        }
    }
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(Dto1 input)
    {
        return input.Prop1.ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReadsAnAutomaticReadWritePropertyOnParameterWhoseTypeIsDefinedInCodeIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public int Prop1 {get; set;} = 5;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(Dto1 input)
    {
        
        return input.Prop1.ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatWritesAnAutomaticReadWritePropertyOnParameterWhoseTypeIsDefinedInCodeIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public int Prop1 {get; set;} = 5;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(Dto1 input)
    {
        input.Prop1 = 6;
        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatIncrementsAnAutomaticReadWritePropertyOnParameterWhoseTypeIsDefinedInCodeIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public int Prop1 {get; set;} = 5;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(Dto1 input)
    {
        input.Prop1 = input.Prop1 + 1;
        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatIncrementsViaPlusPlusAnAutomaticReadWritePropertyOnParameterWhoseTypeIsDefinedInCodeIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public int Prop1 {get; set;} = 5;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(Dto1 input)
    {
        input.Prop1++;
        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatIncrementsViaPlusEqualsAnAutomaticReadWritePropertyOnParameterWhoseTypeIsDefinedInCodeIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public int Prop1 {get; set;} = 5;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(Dto1 input)
    {
        input.Prop1 += 1;
        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
        }
    }
}
