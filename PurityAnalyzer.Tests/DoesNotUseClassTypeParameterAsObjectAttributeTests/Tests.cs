using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.DoesNotUseClassTypeParameterAttributeTests
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void MethodThatDoesNothing_DoesNotUseClassTypeParameterTAsObject()
        {
            string code = @"
using System;

public class DoesNotUseClassTypeParameterAsObjectAttribute : Attribute
{
    public string parameterName;

    public DoesNotUseClassTypeParameterAsObjectAttribute(string parameterName) => this.parameterName = parameterName;
}

public class Class1<T>
{
    [DoesNotUseClassTypeParameterAsObjectAttribute(nameof(T))]
    public void DoSomething(T input)
    {
        
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void MethodThatCallsToStringOnT_UsesClassTypeParameterTAsObject()
        {
            string code = @"
using System;

public class DoesNotUseClassTypeParameterAsObjectAttribute : Attribute
{
    public string parameterName;

    public DoesNotUseClassTypeParameterAsObjectAttribute(string parameterName) => this.parameterName = parameterName;
}

public class Class1<T>
{
    [DoesNotUseClassTypeParameterAsObjectAttribute(nameof(T))]
    public void DoSomething(T input)
    {
        var result = input.ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }


        [Test]
        public void GenericMethodThatCallsToStringOnMethodTypeParameterT_DoesNotUseClassTypeParameterTAsObject()
        {
            string code = @"
using System;

public class DoesNotUseClassTypeParameterAsObjectAttribute : Attribute
{
    public string parameterName;

    public DoesNotUseClassTypeParameterAsObjectAttribute(string parameterName) => this.parameterName = parameterName;
}

public class Class1<T>
{
    [DoesNotUseClassTypeParameterAsObjectAttribute(nameof(T))]
    public void DoSomething<T>(T input)
    {
        var result = input.ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }


        [Test]
        public void MethodThatCallsToStringOnTIndirectlyViaCallingAnotherMethod_UsesClassTypeParameterTAsObject()
        {
            string code = @"
using System;

public class DoesNotUseClassTypeParameterAsObjectAttribute : Attribute
{
    public string parameterName;

    public DoesNotUseClassTypeParameterAsObjectAttribute(string parameterName) => this.parameterName = parameterName;
}

public class Class1<T>
{
    [DoesNotUseClassTypeParameterAsObjectAttribute(nameof(T))]
    public void DoSomething(T input)
    {
        DoSomething2(input);
    }

    public void DoSomething2(T input)
    {
        var result = input.ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void MethodThatCallsAnotherMethodThatDoesNotUseClassTypeParameter_DoesNotUseClassTypeParameterTAsObject()
        {
            string code = @"
using System;

public class DoesNotUseClassTypeParameterAsObjectAttribute : Attribute
{
    public string parameterName;

    public DoesNotUseClassTypeParameterAsObjectAttribute(string parameterName) => this.parameterName = parameterName;
}

public class Class1<T>
{
    [DoesNotUseClassTypeParameterAsObjectAttribute(nameof(T))]
    public void DoSomething(T input)
    {
        DoSomething2(input);
    }

    public void DoSomething2(T input)
    {

    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void DoesNotUseClassTypeParameterAsObjectAttributeAnalysisHandlesRecursionWithoutDeadlocking()
        {
            string code = @"
using System;

public class DoesNotUseClassTypeParameterAsObjectAttribute : Attribute
{
    public string parameterName;

    public DoesNotUseClassTypeParameterAsObjectAttribute(string parameterName) => this.parameterName = parameterName;
}

public class Class1<T>
{
    [DoesNotUseClassTypeParameterAsObjectAttribute(nameof(T))]
    public void DoSomething(T input, int index)
    {
        if(index <= 0)
            return;
        DoSomething(input, index - 1);
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void DoesNotUseClassTypeParameterAsObjectAttributeAnalysisHandlesRecursionWithoutDeadlocking_CaseWhereRecursionIsInMethodNotMarkedWithAttribute()
        {
            string code = @"
using System;

public class DoesNotUseClassTypeParameterAsObjectAttribute : Attribute
{
    public string parameterName;

    public DoesNotUseClassTypeParameterAsObjectAttribute(string parameterName) => this.parameterName = parameterName;
}

public class Class1<T>
{
    [DoesNotUseClassTypeParameterAsObjectAttribute(nameof(T))]
    public void DoSomething(T input)
    {
        DoSomething2(input, 10);
    }

    public void DoSomething2(T input, int index)
    {
        if(index <= 0)
            return;
        DoSomething2(input, index - 1);
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

    }
}

