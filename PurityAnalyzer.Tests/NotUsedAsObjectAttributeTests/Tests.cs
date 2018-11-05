﻿using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.NotUsedAsObjectAttributeTests
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void GenericMethodThatDoesNothing_DoesNotUseTAsObject()
        {
            string code = @"
using System;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1
{
    public static void DoSomething<[NotUsedAsObject] T>(T input)
    {
        
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void GenericMethodThatCallsToStringMethodOnT_UsesTAsObject()
        {
            string code = @"
using System;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1
{
    public static void DoSomething<[NotUsedAsObject] T>(T input)
    {
        var str = input.ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void GenericMethodThatCallsGetHashCodeMethodOnT_UsesTAsObject()
        {
            string code = @"
using System;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1
{
    public static void DoSomething<[NotUsedAsObject] T>(T input)
    {
        var h = input.GetHashCode();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void GenericMethodThatCallsEqualsMethodOnT_UsesTAsObject()
        {
            string code = @"
using System;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1
{
    public static void DoSomething<[NotUsedAsObject] T>(T input)
    {
        var e = input.Equals(new object());
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void GenericMethodThatCallsToStringMethodOnAnotherObjectParam_DoesNotUseTAsObject()
        {
            string code = @"
using System;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1
{
    public static void DoSomething<[NotUsedAsObject] T>(T input, object param)
    {
        var str = param.ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void GenericMethodThatExplicitlyCastsTToObject_UsesTAsObject()
        {
            string code = @"
using System;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1
{
    public static void DoSomething<[NotUsedAsObject] T>(T input)
    {
        object o = (object)input;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void GenericMethodThatImplicitlyCastsTToObject_UsesTAsObject()
        {
            string code = @"
using System;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1
{
    public static void DoSomething<[NotUsedAsObject] T>(T input)
    {
        object o = input;
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void GenericMethodThatImplicitlyCastsTToObjectViaCallingAMethodThatAcceptsObject_UsesTAsObject()
        {
            string code = @"
using System;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1
{
    public static void DoSomething<[NotUsedAsObject] T>(T input)
    {
        DoSomething2(input);
    }

    public static void DoSomething2(object obj)
    {
    }

}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void GenericMethodThatCallsAGenericMethodThatUsesTAsObjectPassingATObjectAsTheOtherMethodT_UsesTAsObject()
        {
            string code = @"
using System;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1
{
    public static void DoSomething<[NotUsedAsObject] T>(T input)
    {
        DoSomething2(input);
    }

    public static void DoSomething2<T>(T obj)
    {
        var s = obj.ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void GenericMethodThatCallsAGenericMethodThatDoesNotUseTAsObjectPassingATObjectAsTheOtherMethodT_DoesNotUseTAsObject()
        {
            string code = @"
using System;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1
{
    public static void DoSomething<[NotUsedAsObject] T>(T input)
    {
        DoSomething2(input);
    }

    public static void DoSomething2<T>(T obj)
    {
        
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }



        [Test]
        public void GenericMethodThatCallsACompiledGenericMethodNotMarkedWithTheNotUsedAsObjectAttributePassingATObjectAsTheOtherMethodT_UsesTAsObject()
        {
            string code = @"
using System;
using PurityAnalyzer.Tests.CompiledCsharpLib;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1
{
    public static void DoSomething<[NotUsedAsObject] T>(T input)
    {
        ClassWithGenericMethods.MethodThatUsesTAsObject(input);
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void GenericMethodThatCallsACompiledGenericMethodMarkedWithTheNotUsedAsObjectAttributePassingATObjectAsTheOtherMethodT_DoesNotUseTAsObject()
        {
            string code = @"
using System;
using PurityAnalyzer.Tests.CompiledCsharpLib;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1
{
    public static void DoSomething<[NotUsedAsObject] T>(T input)
    {
        ClassWithGenericMethods.MethodThatDoesNotUseTAsObject(input);
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void GenericMethodThatCallsToStringMethodOnIEnumerableOfT_DoesNotUseTAsObject()
        {
            string code = @"
using System;
using System.Collections.Generic;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1
{
    public static void DoSomething<[NotUsedAsObject] T>(IEnumerable<T> input)
    {
        var str = input.ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void GenericMethodThatCallsToStringMethodOnFirstElementInIEnumerableOfTUsesTAsObject()
        {
            string code = @"
using System;
using System.Collections.Generic;
using System.Linq;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1
{
    public static void DoSomething<[NotUsedAsObject] T>(IEnumerable<T> input)
    {
        var str = input.First().ToString();
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

    }
}