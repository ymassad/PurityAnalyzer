using FluentAssertions;
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


        [Test]
        public void GenericMethodThatCallsAMethodInAnotherGenericClassThatUsesTAsObjectPassingATObjectAsTheClassT_UsesTAsObject()
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
        Module2<T>.DoSomething2(input);
    }
}

public static class Module2<T>
{
    public static void DoSomething2(T obj)
    {
        var s = obj.ToString();
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void GenericMethodThatCallsAMethodInAGenericClassThatDoesNotUseTAsObjectPassingATObjectAsTheOtherClassT_DoesNotUseTAsObject()
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
        Module2<T>.DoSomething2(input);
    }

}

public static class Module2<T>
{
    public static void DoSomething2(T obj)
    {

    }
}

";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }


        [Test]
        public void GenericMethodThatCallsACompiledMethodInAnotherGenericClassThatUsesTAsObjectPassingATObjectAsTheClassT_UsesTAsObject()
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
        GenericClassAndTIsUsedAsObject<T>.MethodThatUsesTAsObject(input);
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void GenericMethodThatCallsACompiledMethodInAGenericClassThatDoesNotUseTAsObjectPassingATObjectAsTheOtherClassT_DoesNotUseTAsObject()
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
        GenericClassAndTIsNotUsedAsObject<T>.MethodThatDoesNotUseTAsObject(input);
    }

}
";

            var dignostics = Utilities.RunPurityAnalyzer(code, Utilities.GetTestsCompiledCsharpLibProjectReference());
            dignostics.Length.Should().Be(0);
        }


        [Test]
        public void GenericMethodThatCallsAMethodInAnotherClassNestedInAGenericClassThatUsesTAsObjectPassingATObjectAsTheClassT_UsesTAsObject()
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
        Module2<T>.SomeClass.DoSomething2(input);
    }
}

public static class Module2<T>
{
    public static class SomeClass
    {
        public static void DoSomething2(T obj)
        {
            var s = obj.ToString();
        }
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void GenericMethodThatCallsAMethodInAnotherClassNestedInAGenericClassThatDoesNotUseTAsObjectPassingATObjectAsTheOtherClassT_DoesNotUseTAsObject()
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
        Module2<T>.SomeClass.DoSomething2(input);
    }

}

public static class Module2<T>
{
    public static class SomeClass
    {
        public static void DoSomething2(T obj)
        {

        }
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void GenericMethodThatCallsAMethodInAnotherClassNestedInAGenericClass_AndCalledMethodDoesNotUseTAsObject_ButWhereTheClassHasAStaticConstructorThatUsesTAsObject_PassingATObjectAsTheOtherClassT_UsesTAsObject()
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
        Module2<T>.SomeClass.DoSomething2(input);
    }

}

public static class Module2<T>
{
    public static class SomeClass
    {
        static SomeClass()
        {
            var a = default(T).ToString();
        }

        public static void DoSomething2(T obj)
        {

        }
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void GenericMethodThatCallsAMethodInAnotherClassNestedInAGenericClass_AndCalledMethodDoesNotUseTAsObject_ButWhereTheParentClassHasAStaticConstructorThatUsesTAsObject_PassingATObjectAsTheOtherClassT_UsesTAsObject()
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
        Module2<T>.SomeClass.DoSomething2(input);
    }

}

public static class Module2<T>
{
    static Module2()
    {
        var a = default(T).ToString();
    }

    public static class SomeClass
    {
        public static void DoSomething2(T obj)
        {

        }
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void GenericMethodThatCallsAConstructorInAnotherClassNestedInAGenericClass_AndCalledConstructorDoesNotUseTAsObject_ButWhereTheParentClassHasAStaticConstructorThatUsesTAsObject_PassingATObjectAsTheOtherClassT_UsesTAsObject()
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
        var a = new Module2<T>.SomeClass(input);
    }

}

public static class Module2<T>
{
    static Module2()
    {
        var a = default(T).ToString();
    }

    public class SomeClass
    {
        public SomeClass(T obj)
        {

        }
    }
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

    }
}

