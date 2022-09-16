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

            Utilities.AssertPure(code);
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

            Utilities.AssertImpure(code);
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

            Utilities.AssertPure(code);
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

            Utilities.AssertPure(code);
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

            Utilities.AssertImpure(code);
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

            Utilities.AssertImpure(code);
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

            Utilities.AssertImpure(code);
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

            Utilities.AssertImpure(code);
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

            Utilities.AssertImpure(code);
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

            Utilities.AssertImpure(code);
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

            Utilities.AssertPure(code);
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

            Utilities.AssertImpure(code);
        }


        //Reading mutable input should be considered pure
        [Test]
        public void MethodThatReadsAnAutomaticReadWritePropertyOnLambdaParameterWhoseTypeIsDefinedInCodeIsPure()
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
    public static int DoSomething()
    {
        return Do(dto1 => dto1.Prop1);
    }
    
    public static int Do(Func<Dto1,int> func)
    {
        return func(new Dto1());
    }

}";

            Utilities.AssertPure(code);
        }

    }
}
