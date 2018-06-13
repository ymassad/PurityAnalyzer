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
    public class CustomBinaryOperatorsTests
    {
        [TestCaseSource(nameof(GetCases))]
        public void PureCustomBinaryOperatorMethodIsConsideredPure(string op)
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class CustomType
{
    [IsPure]
    public static CustomType operator " + op + @"(CustomType c1, CustomType c2)
    {
        return new CustomType();
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [TestCaseSource(nameof(GetCases))]
        public void ImpureCustomBinaryOperatorMethodIsConsideredImpure(string op)
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
    public static CustomType operator {op}(CustomType c1, CustomType c2)
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
        public void MethodThatUsesPureCustomBinaryOperatorIsPure(string op)
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
        return new CustomType() {op} new CustomType();
    }}
}}

public class CustomType
{{    
    public static CustomType operator {op}(CustomType c1, CustomType c2)
    {{
        return new CustomType();
    }}
}}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [TestCaseSource(nameof(GetCases))]
        public void MethodThatUsesImpureCustomBinaryOperatorIsImpure(string op)
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
        return new CustomType() {op} new CustomType();
    }}
}}

public class CustomType
{{
    static int state = 0;

    public static CustomType operator {op}(CustomType c1, CustomType c2)
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
        public void MethodThatUsesPureCustomBinaryOperatorViaOperatorEqualsIsPure(string op)
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

        a {op}= new CustomType();

        return a;
    }}
}}

public class CustomType
{{    
    public static CustomType operator {op}(CustomType c1, CustomType c2)
    {{
        return new CustomType();
    }}
}}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [TestCaseSource(nameof(GetCases))]
        public void MethodThatUsesImpureCustomBinaryOperatorViaOperatorEqualsIsImpure(string op)
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

        a {op}= new CustomType();

        return a;
    }}
}}

public class CustomType
{{
    static int state = 0;

    public static CustomType operator {op}(CustomType c1, CustomType c2)
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
            yield return "*";
            yield return "/";
            yield return "%";
            yield return "&";
            yield return "^";
            yield return "|";
        }
    }
}
