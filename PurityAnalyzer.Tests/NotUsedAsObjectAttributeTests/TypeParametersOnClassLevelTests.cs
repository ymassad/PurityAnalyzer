using System;
using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.NotUsedAsObjectAttributeTests
{
    [TestFixture]
    public class TypeParametersOnClassLevelTests
    {
        [Test]
        public void MethodThatDoesNothing_DoesNotUseTAsObject()
        {
            string code = @"
using System;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1<[NotUsedAsObject] T>
{
    public static void DoSomething(T input)
    {
        
    }
}";

            Utilities.AssertPure(code);

        }

        [Test]
        public void PropertyThatReturnsSimpleValue_DoesNotUseTAsObject()
        {
            string code = @"
using System;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1<[NotUsedAsObject] T>
{
    public static string Prop1 => string.Empty;
}";

            Utilities.AssertPure(code);
        }



        [Test]
        public void MethodThatCallsToStringMethodOnT_UsesTAsObject()
        {
            string code = @"
using System;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1<[NotUsedAsObject] T>
{
    public static void DoSomething(T input)
    {
        var str = input.ToString();
    }
}";

            Utilities.AssertImpure(code);

        }


        [Test]
        public void StaticConstructorThatCallsToStringMethodOnT_UsesTAsObject()
        {
            string code = @"
using System;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1<[NotUsedAsObject] T>
{
    static Module1()
    {
        var str = default(T).ToString();
    }
}";

            Utilities.AssertImpure(code);

        }

        [Test]
        public void ConstructorThatCallsToStringMethodOnT_UsesTAsObject()
        {
            string code = @"
using System;

public class NotUsedAsObjectAttribute : Attribute
{
}

public class Module1<[NotUsedAsObject] T>
{
    public Module1()
    {
        var str = default(T).ToString();
    }
}";

            Utilities.AssertImpure(code);

        }

        [Test]
        public void PropertyThatCallsToStringMethodOnT_UsesTAsObject()
        {
            string code = @"
using System;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1<[NotUsedAsObject] T>
{
    public static string Prop1 => default(T).ToString();
}";

            Utilities.AssertImpure(code);

        }

        [Test]
        public void IndexerThatCallsToStringMethodOnT_UsesTAsObject()
        {
            string code = @"
using System;

public class NotUsedAsObjectAttribute : Attribute
{
}

public class Module1<[NotUsedAsObject] T>
{
   public string this[T i]
   {
      get { return i.ToString(); }
   }
}";

            Utilities.AssertImpure(code);

        }

        [Test]
        public void MethodThatCallsGetHashCodeMethodOnT_UsesTAsObject()
        {
            string code = @"
using System;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1<[NotUsedAsObject] T>
{
    public static void DoSomething(T input)
    {
        var h = input.GetHashCode();
    }
}";

            Utilities.AssertImpure(code);

        }

        [Test]
        public void MethodThatCallsEqualsMethodOnT_UsesTAsObject()
        {
            string code = @"
using System;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1<[NotUsedAsObject] T>
{
    public static void DoSomething(T input)
    {
        var e = input.Equals(new object());
    }
}";

            Utilities.AssertImpure(code);
        }

        [Test]
        public void MethodThatCallsToStringMethodOnAnotherObjectParam_DoesNotUseTAsObject()
        {
            string code = @"
using System;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1<[NotUsedAsObject] T>
{
    public static void DoSomething(T input, object param)
    {
        var str = param.ToString();
    }
}";

            Utilities.AssertPure(code);

        }

        [Test]
        public void MethodThatExplicitlyCastsTToObject_UsesTAsObject()
        {
            string code = @"
using System;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1<[NotUsedAsObject] T>
{
    public static void DoSomething(T input)
    {
        object o = (object)input;
    }
}";

            Utilities.AssertImpure(code);
        }

        [Test]
        public void MethodThatImplicitlyCastsTToObject_UsesTAsObject()
        {
            string code = @"
using System;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1<[NotUsedAsObject] T>
{
    public static void DoSomething(T input)
    {
        object o = input;
    }
}";

            Utilities.AssertImpure(code);
        }

        [Test]
        public void MethodThatImplicitlyCastsTToObjectViaCallingAMethodThatAcceptsObject_UsesTAsObject()
        {
            string code = @"
using System;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1<[NotUsedAsObject] T>
{
    public static void DoSomething(T input)
    {
        DoSomething2(input);
    }

    public static void DoSomething2(object obj)
    {
    }

}";

            Utilities.AssertImpure(code);
        }

        [Test]
        public void MethodThatCallsAMethodThatUsesT2AsObjectPassingATObjectAsTheOtherMethodT2_UsesTAsObject()
        {
            string code = @"
using System;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1<[NotUsedAsObject] T>
{
    public static void DoSomething(T input)
    {
        DoSomething2(input);
    }

    public static void DoSomething2<T2>(T2 obj)
    {
        var s = obj.ToString();
    }
}";

            Utilities.AssertImpure(code);
        }

        [Test]
        public void MethodThatCallsAMethodInAnotherClassThatUsesT2AsObjectPassingATObjectAsTheOtherClassT2_UsesTAsObject()
        {
            string code = @"
using System;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1<[NotUsedAsObject] T>
{
    public static void DoSomething(T input)
    {
        Module2<T>.DoSomething2(input);
    }
}

public static class Module2<T2>
{
    public static void DoSomething2(T2 obj)
    {
        var s = obj.ToString();
    }
}

";

            Utilities.AssertImpure(code);
        }

        [Test]
        public void MethodThatCallsAMethodThatDoesNotUseTAsObjectPassingATObjectAsTheOtherMethodT_DoesNotUseTAsObject()
        {
            string code = @"
using System;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1<[NotUsedAsObject] T>
{
    public static void DoSomething(T input)
    {
        DoSomething2(input);
    }

    public static void DoSomething2<T2>(T2 obj)
    {
        
    }
}";

            Utilities.AssertPure(code);
        }



        [Test]
        public void MethodThatCallsACompiledGenericMethodNotMarkedWithTheNotUsedAsObjectAttributePassingATObjectAsTheOtherMethodT_UsesTAsObject()
        {
            string code = @"
using System;
using PurityAnalyzer.Tests.CompiledCsharpLib;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1<[NotUsedAsObject] T>
{
    public static void DoSomething(T input)
    {
        ClassWithGenericMethods.MethodThatUsesTAsObject(input);
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().BePositive();
        }


        [Test]
        public void MethodThatCallsACompiledMethodInAGenericClassWhereTIsNotMarkedWithTheNotUsedAsObjectAttributePassingATObjectAsTheOtherClassT_UsesTAsObject()
        {
            string code = @"
using System;
using PurityAnalyzer.Tests.CompiledCsharpLib;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1<[NotUsedAsObject] T>
{
    public static void DoSomething(T input)
    {
        GenericClassAndTIsUsedAsObject<T>.MethodThatUsesTAsObject(input);
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void MethodThatCallsACompiledGenericMethodMarkedWithTheNotUsedAsObjectAttributePassingATObjectAsTheOtherMethodT_DoesNotUseTAsObject()
        {
            string code = @"
using System;
using PurityAnalyzer.Tests.CompiledCsharpLib;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1<[NotUsedAsObject] T>
{
    public static void DoSomething(T input)
    {
        ClassWithGenericMethods.MethodThatDoesNotUseTAsObject(input);
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsACompiledMethodInAGenericClassWhereTIsMarkedWithTheNotUsedAsObjectAttributePassingATObjectAsTheOtherMethodT_DoesNotUseTAsObject()
        {
            string code = @"
using System;
using PurityAnalyzer.Tests.CompiledCsharpLib;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1<[NotUsedAsObject] T>
{
    public static void DoSomething(T input)
    {
        GenericClassAndTIsNotUsedAsObject<T>.MethodThatDoesNotUseTAsObject(input);
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void MethodThatCallsToStringMethodOnIEnumerableOfT_DoesNotUseTAsObject()
        {
            string code = @"
using System;
using System.Collections.Generic;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1<[NotUsedAsObject] T>
{
    public static void DoSomething(IEnumerable<T> input)
    {
        var str = input.ToString();
    }
}";

            Utilities.AssertPure(code);

        }

        [Test]
        public void MethodThatCallsToStringMethodOnFirstElementInIEnumerableOfTUsesTAsObject()
        {
            string code = @"
using System;
using System.Collections.Generic;
using System.Linq;

public class NotUsedAsObjectAttribute : Attribute
{
}

public static class Module1<[NotUsedAsObject] T>
{
    public static void DoSomething(IEnumerable<T> input)
    {
        var str = input.First().ToString();
    }
}";

            Utilities.AssertImpure(code);

        }

    }
}
