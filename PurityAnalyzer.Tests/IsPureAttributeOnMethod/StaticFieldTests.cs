using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod
{
    [TestFixture]
    public class StaticFieldTests
    {

        [Test]
        public void MethodThatSimplyReturnsAnEmptyStringIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    public static int state;
    [IsPure]
    public static string DoSomething()
    {
        return """";
    }
}";

            Utilities.AssertPure(code);
            
        }

        [Test]
        public void MethodThatMutatesStaticFieldIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    public static int state;
    [IsPure]
    public static string DoSomething()
    {
        state = 6;
        return """";
    }
}";

            Utilities.AssertImpure(code);
        }

        [Test]
        public void MethodThatReadsAndMutatesStaticFieldIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    public static int state;
    [IsPure]
    public static string DoSomething()
    {
        state = state + 6;
        return """";
    }
}";

            Utilities.AssertImpure(code);
        }

        [Test]
        public void MethodThatIncrementsStaticFieldIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    public static int state;
    [IsPure]
    public static string DoSomething()
    {
        state++;
        return """";
    }
}";

            Utilities.AssertImpure(code);
        }

        [Test]
        public void MethodThatIncrementsStaticFieldViaPlusEqualsIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    public static int state;
    [IsPure]
    public static string DoSomething()
    {
        state += 1;
        return """";
    }
}";

            Utilities.AssertImpure(code);
        }

        [Test]
        public void MethodThatDecrementsStaticFieldIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    public static int state;
    [IsPure]
    public static string DoSomething()
    {
        state--;
        return """";
    }
}";

            Utilities.AssertImpure(code);
        }

        [Test]
        public void MethodThatDecrementsStaticFieldViaMinusEqualsIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    public static int state;
    [IsPure]
    public static string DoSomething()
    {
        state -= 1;
        return """";
    }
}";

            Utilities.AssertImpure(code);
        }


        [Test]
        public void MethodThatReadsNonReadOnlyStaticFieldIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    public static int state;
    [IsPure]
    public static string DoSomething()
    {
        return state.ToString();
    }
}";
            Utilities.AssertImpure(code);
        }

        [Test]
        public void MethodThatReadsReadOnlyStaticFieldIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    public static readonly int state = 1;
    [IsPure]
    public static int DoSomething()
    {
        
        return state;
    }
}";

            Utilities.AssertPure(code);
        }
    }

}
