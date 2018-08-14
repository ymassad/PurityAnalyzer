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
    public class CustomUnaryOperatorsTests
    {
        [TestCaseSource(nameof(GetCases))]
        public void PureCustomUnaryOperatorMethodIsConsideredPure(string op)
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class CustomType
{
    [IsPure]
    public static CustomType operator " + op + @"(CustomType c1)
    {
        return new CustomType();
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [TestCaseSource(nameof(GetCases))]
        public void ImpureCustomUnaryOperatorMethodIsConsideredImpure(string op)
        {
            string code = $@"
using System;

public class IsPureAttribute : Attribute
{{
}}

public class CustomType
{{
    static int state = 0;

    [IsPure]
    public static CustomType operator {op}(CustomType c1)
    {{
        state++;
        return new CustomType();
    }}
}}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [TestCaseSource(nameof(GetCases))]
        public void MethodThatUsesPureCustomUnaryOperator_Prefix_IsPure(string op)
        {
            string code = $@"
using System;

public class IsPureAttribute : Attribute
{{
}}

public class MyClass
{{
    [IsPure]
    public static CustomType DoSomething()
    {{
        var a = new CustomType();
        return {op}a;
    }}
}}

public class CustomType
{{    
    public static CustomType operator {op}(CustomType c1)
    {{
        return new CustomType();
    }}
}}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [TestCaseSource(nameof(GetCases))]
        public void MethodThatUsesImpureCustomUnaryOperator_Prefix_IsImpure(string op)
        {
            string code = $@"
using System;

public class IsPureAttribute : Attribute
{{
}}

public class MyClass
{{
    [IsPure]
    public static CustomType DoSomething()
    {{
        var a = new CustomType();
        return {op}a;
    }}
}}

public class CustomType
{{
    static int state = 0;

    public static CustomType operator {op}(CustomType c1)
    {{
        state++;
        return new CustomType();
    }}
}}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [TestCaseSource(nameof(GetCases2))]
        public void MethodThatUsesPureCustomUnaryOperator_Postfix_IsPure(string op)
        {
            string code = $@"
using System;

public class IsPureAttribute : Attribute
{{
}}

public class MyClass
{{
    [IsPure]
    public static CustomType DoSomething()
    {{
        var a = new CustomType();
        return a{op};
    }}
}}

public class CustomType
{{    
    public static CustomType operator {op}(CustomType c1)
    {{
        return new CustomType();
    }}
}}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [TestCaseSource(nameof(GetCases2))]
        public void MethodThatUsesImpureCustomUnaryOperator_Postfix_IsImpure(string op)
        {
            string code = $@"
using System;

public class IsPureAttribute : Attribute
{{
}}

public class MyClass
{{
    [IsPure]
    public static CustomType DoSomething()
    {{
        var a = new CustomType();
        return a{op};
    }}
}}

public class CustomType
{{
    static int state = 0;

    public static CustomType operator {op}(CustomType c1)
    {{
        state++;
        return new CustomType();
    }}
}}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }


        private static IEnumerable<string> GetCases()
        {
            yield return "+";
            yield return "-";
            yield return "--";
            yield return "++";
            yield return "!";
            yield return "~";
        }

        private static IEnumerable<string> GetCases2()
        {
            yield return "--";
            yield return "++";
        }
    }
}
