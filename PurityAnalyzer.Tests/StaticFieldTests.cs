﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace PurityAnalyzer.Tests
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

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
            
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

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
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

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
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

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
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

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
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

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
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

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
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
            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().BePositive();
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
    public static string DoSomething()
    {
        
        return state.ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);

            dignostics.Length.Should().Be(0);
        }
    }

}
