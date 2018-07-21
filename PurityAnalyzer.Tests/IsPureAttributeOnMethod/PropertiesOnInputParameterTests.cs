using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod
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
    public static int DoSomething(Dto1 input)
    {
        
        return input.Prop1;
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
    public static int DoSomething(Dto1 input)
    {
        return input.Prop1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }

        //Reading mutable input should be considered pure
        [Test]
        public void MethodThatReadsAnAutomaticReadWritePropertyOnParameterWhoseTypeIsDefinedInCodeIsPure()
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
    public static int DoSomething(Dto1 input)
    {
        return input.Prop1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }


        [Test]
        public void MethodThatReadsANonPureReadWritePropertyOnParameterWhoseTypeIsDefinedInCodeIsImpure()
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
            return state;
        }
        set
        {
            state = value;
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
        public void MethodThatReadsANonPureReadOnlyPropertyOnParameterWhoseTypeIsDefinedInCodeIsImpure()
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
            return state;
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



        [Test]
        public void MethodThatReadsAnReadOnlyPropertyThatInvokesAMethodThatReadsMutableStateOnParameterWhoseTypeIsDefinedInCodeIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    int state = 0;
    
    int Method() => state;

    public int Prop1 => Method();
}

public static class Module1
{
    [IsPure]
    public static int DoSomething(Dto1 input)
    {
        return input.Prop1;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatReadsAnReadOnlyPropertyThatInvokesAMethodThatWritesMutableStateOnParameterWhoseTypeIsDefinedInCodeIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    int state = 0;
    
    int Method()
    {
        state = 1;
        return 1;
    }

    public int Prop1 => Method();
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

    }
}
