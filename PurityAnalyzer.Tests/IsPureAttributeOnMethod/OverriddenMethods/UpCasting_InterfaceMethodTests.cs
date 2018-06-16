using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod.OverriddenMethods
{
    [TestFixture]
    public class UpCasting_InterfaceMethodTests
    {
        [Test]
        public void UpCastingFromObjectToInterfaceMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public interface IInterface
{
    int Method();
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(object obj)
    {
        var v = (IInterface)obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void UpCastingFromObjectToTypeThatImplementsAnInterfaceMethodWithAMethodThatIsImpureKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public interface IInterface
{
    int Method();
}

public class Derived : IInterface
{
    static int state = 0;
    public int Method() => state++;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(object obj)
    {
        var v = (Derived)obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void UpCastingFromObjectToTypeThatExplicitlyImplementsAnInterfaceMethodWithAMethodThatIsImpureKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public interface IInterface
{
    int Method();
}

public class Derived : IInterface
{
    static int state = 0;
    int IInterface.Method() => state++;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(object obj)
    {
        var v = (Derived)obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }


        [Test]
        public void UpCastingFromObjectToTypeThatImplementsAnInterfaceMethodWithAMethodThatIsPureMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public interface IInterface
{
    int Method();
}

public class Derived : IInterface
{
    public int Method() => 2;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(object obj)
    {
        var v = (Derived)obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void UpCastingFromObjectToTypeThatExplicitlyImplementsAnInterfaceMethodWithAMethodThatIsPureMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public interface IInterface
{
    int Method();
}

public class Derived : IInterface
{
    int IInterface.Method() => 2;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(object obj)
    {
        var v = (Derived)obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }


        [Test]
        public void UpCastingFromObjectToSealedTypeThatImplementsAnInterfaceMethodWithAMethodThatIsPureKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public interface IInterface
{
    int Method();
}

public sealed class Derived : IInterface
{
    public int Method() => 2;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(object obj)
    {
        var v = (Derived)obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void UpCastingFromObjectToSealedTypeThatExplicitlyImplementsAnInterfaceMethodWithAMethodThatIsPureKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public interface IInterface
{
    int Method();
}

public sealed class Derived : IInterface
{
    int IInterface.Method() => 2;
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(object obj)
    {
        var v = (Derived)obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void UpCastingFromObjectToTypeWhoseSubTypeImplementsAnInterfaceMethodWithAMethodThatIsImpureKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public interface IInterface
{
    int Method();
}

public class Middle : IInterface
{
    static int state = 0;
    public int Method() => state++;
}

public class Derived : Middle
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(object obj)
    {
        var v = (Derived)obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void UpCastingFromObjectToTypeWhoseSubTypeExplicitlyImplementsAnInterfaceMethodWithAMethodThatIsImpureKeepsMethodPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public interface IInterface
{
    int Method();
}

public class Middle : IInterface
{
    static int state = 0;
    int IInterface.Method() => state++;
}

public class Derived : Middle
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(object obj)
    {
        var v = (Derived)obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }


        [Test]
        public void UpCastingFromObjectToTypeWhoseSubTypeImplementsAnInterfaceMethodWithAMethodThatIsPureMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public interface IInterface
{
    int Method();
}

public class Middle : IInterface
{
    public int Method() => 2;
}

public class Derived : Middle
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(object obj)
    {
        var v = (Derived)obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void UpCastingFromObjectToTypeWhoseSubTypeExplicitlyImplementsAnInterfaceMethodWithAMethodThatIsPureMakesMethodImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public interface IInterface
{
    int Method();
}

public class Middle : IInterface
{
    int IInterface.Method() => 2;
}

public class Derived : Middle
{
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(object obj)
    {
        var v = (Derived)obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }
    }
}
