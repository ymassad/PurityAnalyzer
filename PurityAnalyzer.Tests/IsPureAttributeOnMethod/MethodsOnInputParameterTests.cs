using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod
{
    [TestFixture]
    public class MethodsOnInputParameterTests
    {
        [Test]
        public void MethodThatInvokesAMethodThatReadsAnAutomaticReadOnlyPropertyOnParameterWhoseTypeIsDefinedInCodeIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public int Prop1 {get;} = 5;
    
    public int Method() => Prop1;

}

public static class Module1
{
    [IsPure]
    public static int DoSomething(Dto1 input)
    {
        return input.Method();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatInvokesAMethodThatReadsAnAutomaticReadWritePropertyOnParameterWhoseTypeIsDefinedInCodeIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public int Prop1 {get; set;} = 5;
    
    public int Method() => Prop1;

}

public static class Module1
{
    [IsPure]
    public static int DoSomething(Dto1 input)
    {
        return input.Method();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatInvokesAMethodThatWritesAnAutomaticReadWritePropertyOnParameterWhoseTypeIsDefinedInCodeIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public int Prop1 {get; set;} = 5;
    
    public int Method()
    {
        Prop1 = 1;
        return 1;
    }
}

public static class Module1
{
    [IsPure]
    public static int DoSomething(Dto1 input)
    {
        return input.Method();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatInvokesAMethodThatReadsAReadOnlyFieldOnParameterWhoseTypeIsDefinedInCodeIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public readonly int Field = 5;
    
    public int Method() => Field;
}

public static class Module1
{
    [IsPure]
    public static int DoSomething(Dto1 input)
    {
        return input.Method();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatInvokesAMethodThatReadsAnReadWriteFieldOnParameterWhoseTypeIsDefinedInCodeIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public int Field = 5;
    
    public int Method() => Field;
}

public static class Module1
{
    [IsPure]
    public static int DoSomething(Dto1 input)
    {
        return input.Method();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatInvokesAMethodThatWritesAnReadWriteFieldOnParameterWhoseTypeIsDefinedInCodeIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public int Field = 5;
    
    public int Method()
    {
        Field = 1;
        return 1;
    }
}

public static class Module1
{
    [IsPure]
    public static int DoSomething(Dto1 input)
    {
        return input.Method();
    }
}";
            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatInvokesAMethodThatdReadsAnNonAutomaticReadOnlyPropertyThatModifiesStateOnParameterWhoseTypeIsDefinedInCodeIsImpure()
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

    public int Method() => Prop1;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(Dto1 input)
    {
        
        return input.Method().ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatInvokesAMethodThatInvokesAnotherMethodThatReadsMutableStateOnParameterWhoseTypeIsDefinedInCodeIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    int state = 0;

    public int Method() => Method2();
    public int Method2() => state;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(Dto1 input)
    {
        
        return input.Method().ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatInvokesAMethodThatInvokesAnotherMethodThatWritesMutableStateOnParameterWhoseTypeIsDefinedInCodeIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    int state = 0;

    public int Method() => Method2();
    public int Method2()
    {
        state = 2;
        return 1;
    }
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(Dto1 input)
    {
        return input.Method().ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatInvokesAMethodThatInvokesAnotherMethodThatReadsMutableStateOnParameterWhoseTypeIsDefinedInCodeIndirectlyViaVariableIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    int state = 0;

    public int Method() => Method2();
    public int Method2() => state;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(Dto1 input)
    {
        var v = input;
        return v.Method().ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatInvokesAMethodThatInvokesAnotherMethodThatReadsMutableStateOnParameterWhoseTypeIsDefinedInCodeIndirectlyViaTwoVariablesIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    int state = 0;

    public int Method() => Method2();
    public int Method2() => state;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(Dto1 input)
    {
        var v = input;
        var v2 = v;
        return v2.Method().ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatInvokesAMethodThatInvokesAnotherMethodThatWritesMutableStateOnParameterWhoseTypeIsDefinedInCodeIndirectlyViaVariableIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    int state = 0;

    public int Method() => Method2();
    public int Method2()
    {
        state = 2;
        return 1;
    }
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(Dto1 input)
    {
        var v = input;
        return v.Method().ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatInvokesAMethodThatInvokesAnotherMethodThatWritesMutableStateOnParameterWhoseTypeIsDefinedInCodeIndirectlyViaTwoVariablesIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    int state = 0;

    public int Method() => Method2();
    public int Method2()
    {
        state = 2;
        return 1;
    }
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(Dto1 input)
    {
        var v = input;
        var v2 = v;
        return v2.Method().ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatInvokesAMethodThatReadsAnReadWriteFieldOnObjectObtainedViaReadOnlyFieldOnParameterWhoseTypeIsDefinedInCodeIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public int Field = 5;
    
    public int Method() => Field;
}

public class Dto0
{
    public readonly Dto1 Dto = new Dto1();
}

public static class Module1
{
    [IsPure]
    public static int DoSomething(Dto0 input)
    {
        return input.Dto.Method();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatInvokesAMethodThatWritesAnReadWriteFieldOnObjectObtainedViaReadOnlyFieldOnParameterWhoseTypeIsDefinedInCodeIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public int Field = 5;
    
    public int Method()
    {
        Field = 1;
        return 1;
    }
}

public class Dto0
{
    public readonly Dto1 Dto = new Dto1();
}

public static class Module1
{
    [IsPure]
    public static int DoSomething(Dto0 input)
    {
        return input.Dto.Method();
    }
}";
            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatInvokesAMethodThatReadsAnReadWriteFieldOnObjectObtainedViaReadOnlyAutoPropertyOnParameterWhoseTypeIsDefinedInCodeIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public int Field = 5;
    
    public int Method() => Field;
}

public class Dto0
{
    public Dto1 Dto { get; } = new Dto1();
}

public static class Module1
{
    [IsPure]
    public static int DoSomething(Dto0 input)
    {
        return input.Dto.Method();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatInvokesAMethodThatWritesAnReadWriteFieldOnObjectObtainedViaReadOnlyAutoPropertyOnParameterWhoseTypeIsDefinedInCodeIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public int Field = 5;
    
    public int Method()
    {
        Field = 1;
        return 1;
    }
}

public class Dto0
{
    public Dto1 Dto {get;} = new Dto1();
}

public static class Module1
{
    [IsPure]
    public static int DoSomething(Dto0 input)
    {
        return input.Dto.Method();
    }
}";
            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatInvokesAMethodThatReadsAnReadWriteFieldOnObjectObtainedViaReadWriteFieldOnParameterWhoseTypeIsDefinedInCodeIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public int Field = 5;
    
    public int Method() => Field;
}

public class Dto0
{
    public Dto1 Dto = new Dto1();
}

public static class Module1
{
    [IsPure]
    public static int DoSomething(Dto0 input)
    {
        return input.Dto.Method();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatInvokesAMethodThatWritesAnReadWriteFieldOnObjectObtainedViaReadWriteFieldOnParameterWhoseTypeIsDefinedInCodeIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public int Field = 5;
    
    public int Method()
    {
        Field = 1;
        return 1;
    }
}

public class Dto0
{
    public Dto1 Dto = new Dto1();
}

public static class Module1
{
    [IsPure]
    public static int DoSomething(Dto0 input)
    {
        return input.Dto.Method();
    }
}";
            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatInvokesAMethodThatReadsAnReadWriteFieldOnObjectObtainedViaReadWriteAutoPropertyOnParameterWhoseTypeIsDefinedInCodeIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public int Field = 5;
    
    public int Method() => Field;
}

public class Dto0
{
    public Dto1 Dto { get; set; } = new Dto1();
}

public static class Module1
{
    [IsPure]
    public static int DoSomething(Dto0 input)
    {
        return input.Dto.Method();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatInvokesAMethodThatWritesAnReadWriteFieldOnObjectObtainedViaReadWriteAutoPropertyOnParameterWhoseTypeIsDefinedInCodeIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public int Field = 5;
    
    public int Method()
    {
        Field = 1;
        return 1;
    }
}

public class Dto0
{
    public Dto1 Dto {get; set; } = new Dto1();
}

public static class Module1
{
    [IsPure]
    public static int DoSomething(Dto0 input)
    {
        return input.Dto.Method();
    }
}";
            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
        }


        [Test]
        public void MethodThatInvokesAMethodThatReadsAnReadWriteFieldOnObjectObtainedViaReadOnlyPropertyThatReturnsStaticObjectOnParameterWhoseTypeIsDefinedInCodeIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Dto1
{
    public int Field = 5;
    
    public int Method() => Field;
}

public class Dto0
{
    public Dto1 Dto => dto;

    static Dto1 dto = new Dto1();
}

public static class Module1
{
    [IsPure]
    public static int DoSomething(Dto0 input)
    {
        return input.Dto.Method();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
        }
    }
}
