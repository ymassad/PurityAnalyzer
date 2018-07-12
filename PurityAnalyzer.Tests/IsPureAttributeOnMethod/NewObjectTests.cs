using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod
{
    [TestFixture]
    public class NewObjectTests
    {
        [Test]
        public void CreatingAnInstanceOfAClassWithPureConstructorKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class PureDto
{
    public int Age {get;}

    public PureDto(int age) => Age = age;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CreatingAnInstanceOfAClassThatHasAnImpureInstanceConstructorMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class PureDto
{
    public int Age {get;}

    static int state = 0;

    public PureDto(int age) { state++; Age = age;}
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CreatingAnInstanceOfAClassThatHasAnImpureInstanceFieldInitializerMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class PureDto
{
    public int Age {get;}

    int state = Utils.ImpureMethod();

    public PureDto(int age) { Age = age;}
}

public static class Utils
{
    static int state = 0;
    public static int ImpureMethod() => state++;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CreatingAnInstanceOfAClassThatHasAnImpureInstancePropertyInitializerMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class PureDto
{
    public int Age {get;}

    int state {get;} = Utils.ImpureMethod();

    public PureDto(int age) { Age = age;}
}

public static class Utils
{
    static int state = 0;
    public static int ImpureMethod() => state++;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CreatingAnInstanceOfAClassThatHasAnImpureStaticFieldInitializerMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class PureDto
{
    public int Age {get;}

    static int state = Utils.ImpureMethod();

    public PureDto(int age) { Age = age;}
}

public static class Utils
{
    static int state = 0;
    public static int ImpureMethod() => state++;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CreatingAnInstanceOfAClassThatHasAnImpureStaticPropertyInitializerMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class PureDto
{
    public int Age {get;}

    static int state {get;} = Utils.ImpureMethod();

    public PureDto(int age) { Age = age;}
}

public static class Utils
{
    static int state = 0;
    public static int ImpureMethod() => state++;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CreatingAnInstanceOfAClassThatHasAnImpureStaticConstructorMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class PureDto
{
    public int Age {get;}

    static PureDto()
    {
        AnotherClass.state++;
    }

    public PureDto(int age) { Age = age;}
}

public static class AnotherClass
{
    public static int state = 0;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CreatingAnInstanceOfAClassThatHasAnImpureStaticMethodKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class PureDto
{
    public int Age {get;}

    static int state = 0;

    public static int Method() => state++;

    public PureDto(int age) { Age = age;}
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }


        [Test]
        public void CreatingAnInstanceOfAClassThatHasAnImpureBaseInstanceConstructorMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    static int state = 0;

    public Base() { state++;}
}

public class PureDto : Base
{
    public int Age {get;}

    public PureDto(int age) { Age = age;}
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CreatingAnInstanceOfAClassThatHasAnImpureBaseInstanceFieldInitializerMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    static int state = 0;

    int localState = state++;
}

public class PureDto : Base
{
    public int Age {get;}

    public PureDto(int age) { Age = age;}
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CreatingAnInstanceOfAClassThatHasAnImpureBaseInstancePropertyInitializerMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    static int state = 0;

    int localState {get;} = state++;
}

public class PureDto : Base
{
    public int Age {get;}

    public PureDto(int age) { Age = age;}
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CreatingAnInstanceOfAClassInAnotherFileWithPureConstructorKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var code2 = @"
public class PureDto
{
    public int Age {get;}

    public PureDto(int age) => Age = age;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code, code2);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CreatingAnInstanceOfAClassInAnotherFileThatHasAnImpureInstanceConstructorMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";


            var code2 = @"
public class PureDto
{
    public int Age {get;}

    static int state = 0;

    public PureDto(int age) { state++; Age = age;}
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, code2);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CreatingAnInstanceOfAClassInAnotherFileThatHasAnImpureInstanceFieldInitializerMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var code2 = @"
public class PureDto
{
    public int Age {get;}

    int state = Utils.ImpureMethod();

    public PureDto(int age) { Age = age;}
}

public static class Utils
{
    static int state = 0;
    public static int ImpureMethod() => state++;
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, code2);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CreatingAnInstanceOfAClassInAnotherFileThatHasAnImpureInstancePropertyInitializerMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var code2 = @"
public class PureDto
{
    public int Age {get;}

    int state {get;} = Utils.ImpureMethod();

    public PureDto(int age) { Age = age;}
}

public static class Utils
{
    static int state = 0;
    public static int ImpureMethod() => state++;
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, code2);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CreatingAnInstanceOfAClassInAnotherFileThatHasAnImpureStaticFieldInitializerMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var code2 = @"
public class PureDto
{
    public int Age {get;}

    static int state = Utils.ImpureMethod();

    public PureDto(int age) { Age = age;}
}

public static class Utils
{
    static int state = 0;
    public static int ImpureMethod() => state++;
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, code2);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CreatingAnInstanceOfAClassInAnotherFileThatHasAnImpureStaticPropertyInitializerMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var code2 = @"
public class PureDto
{
    public int Age {get;}

    static int state {get;} = Utils.ImpureMethod();

    public PureDto(int age) { Age = age;}
}

public static class Utils
{
    static int state = 0;
    public static int ImpureMethod() => state++;
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, code2);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CreatingAnInstanceOfAClassInAnotherFileThatHasAnImpureStaticConstructorMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var code2 = @"
public class PureDto
{
    public int Age {get;}

    static PureDto()
    {
        AnotherClass.state++;
    }

    public PureDto(int age) { Age = age;}
}

public static class AnotherClass
{
    public static int state = 0;
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, code2);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CreatingAnInstanceOfAClassInAnotherFileThatHasAnImpureStaticMethodKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var code2 = @"
public class PureDto
{
    public int Age {get;}

    static int state = 0;

    public static int Method() => state++;

    public PureDto(int age) { Age = age;}
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, code2);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void CreatingAnInstanceOfAClassInAnotherFileThatHasAnImpureBaseInstanceConstructorMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var code2 = @"
public class Base
{
    static int state = 0;

    public Base() { state++;}
}

public class PureDto : Base
{
    public int Age {get;}

    public PureDto(int age) { Age = age;}
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, code2);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CreatingAnInstanceOfAClassInAnotherFileThatHasAnImpureBaseInstanceFieldInitializerMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var code2 = @"
public class Base
{
    static int state = 0;

    int localState = state++;
}

public class PureDto : Base
{
    public int Age {get;}

    public PureDto(int age) { Age = age;}
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code, code2);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CreatingAnInstanceOfAClassInAnotherFileThatHasAnImpureBaseInstancePropertyInitializerMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var code2 = @"
public class Base
{
    static int state = 0;

    int localState {get;} = state++;
}

public class PureDto : Base
{
    public int Age {get;}

    public PureDto(int age) { Age = age;}
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, code2);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CreatingAnInstanceOfAClassThatHasAnImpureBaseInstanceConstructorAndBaseClassIsInAnotherFileMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class PureDto : Base
{
    public int Age {get;}

    public PureDto(int age) { Age = age;}
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var code2 = @"
public class Base
{
    static int state = 0;

    public Base() { state++;}
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, code2);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CreatingAnInstanceOfAClassThatHasAnImpureBaseInstanceFieldInitializerAndBaseClassIsInAnotherFileMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class PureDto : Base
{
    public int Age {get;}

    public PureDto(int age) { Age = age;}
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var code2 = @"
public class Base
{
    static int state = 0;

    int localState = state++;
}
";

            var dignostics = Utilities.RunPurityAnalyzer(code, code2);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CreatingAnInstanceOfAClassThatHasAnImpureBaseInstancePropertyInitializerAndBaseClassIsInAnotherFileMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class PureDto : Base
{
    public int Age {get;}

    public PureDto(int age) { Age = age;}
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new PureDto(1);

        return """";
    }
}";

            var code2 = @"
public class Base
{
    static int state = 0;

    int localState {get;} = state++;
}";

            var dignostics = Utilities.RunPurityAnalyzer(code, code2);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void CreatingAnInstanceOfTheObjectTypeKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething()
    {
        var obj = new object();

        return """";
    }
}";


            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

    }
}
