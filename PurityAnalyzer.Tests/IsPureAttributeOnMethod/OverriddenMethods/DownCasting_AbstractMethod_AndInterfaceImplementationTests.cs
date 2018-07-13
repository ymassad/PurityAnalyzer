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
    public class DownCasting_AbstractMethod_AndInterfaceImplementationTests
    {
        [Test]
        public void DownCastingFromObjectToTypeWithAbstractMethodThatImplementsAnInterfaceMakesMethodImpure()
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

public abstract class Base : IInterface
{
    public abstract int Method();
}

public static class Module1
{
    [IsPure]
    public static string DoSomething(object obj)
    {
        var v = (Base)obj;

        return """";
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void DownCastingFromObjectToTypeThatOverridesAnAbstractMethodThatImplementsAnInterfaceWithAMethodThatIsImpureKeepsMethodPure()
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

public abstract class Base : IInterface
{
    public abstract int Method();
}

public class Derived : Base
{
    static int state = 0;
    public override int Method() => state++;
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
        public void DownCastingFromObjectToTypeThatOverridesAnAbstractMethodWithAMethodThatIsImpureAndThatImplementsAnInterfaceKeepsMethodPure()
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

public abstract class Base
{
    public abstract int Method();
}

public class Derived : Base, IInterface
{
    static int state = 0;
    public override int Method() => state++;
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
        public void DownCastingFromObjectToTypeThatOverridesAnAbstractMethodThatImplementsAnInterfaceWithAMethodThatIsPureMakesMethodImpure()
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

public abstract class Base : IInterface
{
    public abstract int Method();
}

public class Derived : Base
{
    public override int Method() => 2;
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
        public void DownCastingFromObjectToTypeThatOverridesAnAbstractMethodWithAMethodThatIsPureAndThatImplementsAnInterfaceMakesMethodImpure()
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

public abstract class Base
{
    public abstract int Method();
}

public class Derived : Base, IInterface
{
    public override int Method() => 2;
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
        public void DownCastingFromObjectToSealedTypeThatOverridesAnAbstractMethodThatImplementsAnInterfaceWithAMethodThatIsPureKeepsMethodPure()
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

public abstract class Base : IInterface
{
    public abstract int Method();
}

public sealed class Derived : Base
{
    public override int Method() => 2;
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
        public void DownCastingFromObjectToSealedTypeThatOverridesAnAbstractMethodWithAMethodThatIsPureAndThatImplementsAnInterfaceKeepsMethodPure()
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

public abstract class Base
{
    public abstract int Method();
}

public sealed class Derived : Base, IInterface
{
    public override int Method() => 2;
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
        public void DownCastingFromObjectToTypeThatOverridesAnAbstractMethodThatImplementsAnInterfaceWithASealedMethodThatIsPureKeepsMethodPure()
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

public abstract class Base : IInterface
{
    public abstract int Method();
}

public class Derived : Base
{
    public sealed override int Method() => 2;
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
        public void DownCastingFromObjectToTypeThatOverridesAnAbstractMethodWithASealedMethodThatIsPureAndThatImplementsAnInterfaceKeepsMethodPure()
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

public abstract class Base
{
    public abstract int Method();
}

public class Derived : Base, IInterface
{
    public sealed override int Method() => 2;
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
        public void DownCastingFromObjectToTypeThatOverridesAnAbstractMethodThatImplementsAnInterfaceWithASealedMethodThatIsImpureKeepsMethodPure()
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

public abstract class Base : IInterface
{
    public abstract int Method();
}

public class Derived : Base
{
    static int state = 0;
    public sealed override int Method() => state++;
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
        public void DownCastingFromObjectToTypeThatOverridesAnAbstractMethodWithASealedMethodThatIsImpureAndThatImplementsAnInterfaceKeepsMethodPure()
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

public abstract class Base
{
    public abstract int Method();
}

public class Derived : Base, IInterface
{
    static int state = 0;
    public sealed override int Method() => state++;
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
        public void DownCastingFromObjectToTypeWhoseSubTypeOverridesAnAbstractMethodThatImplementsAnInterfaceWithAMethodThatIsImpureKeepsMethodPure()
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

public abstract class Base : IInterface
{
    public abstract int Method();
}

public class Middle : Base
{
    static int state = 0;
    public override int Method() => state++;
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
        public void DownCastingFromObjectToTypeWhoseSubTypeOverridesAnAbstractMethodWithAMethodThatIsImpureAndThatImplementsAnInterfaceKeepsMethodPure()
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

public abstract class Base
{
    public abstract int Method();
}

public class Middle : Base, IInterface
{
    static int state = 0;
    public override int Method() => state++;
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
        public void DownCastingFromObjectToTypeWhoseSubTypeOverridesAnAbstractMethodWithAMethodThatIsImpureAndTypeImplementsAnInterfaceKeepsMethodPure()
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

public abstract class Base
{
    public abstract int Method();
}

public class Middle : Base
{
    static int state = 0;
    public override int Method() => state++;
}

public class Derived : Middle, IInterface
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
        public void DownCastingFromObjectToTypeWhoseSubTypeOverridesAnAbstractMethodThatImplementsAnInterfaceWithAMethodThatIsPureMakesMethodImpure()
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

public abstract class Base : IInterface
{
    public abstract int Method();
}

public class Middle : Base
{
    public override int Method() => 2;
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
        public void DownCastingFromObjectToTypeWhoseSubTypeOverridesAnAbstractMethodWithAMethodThatIsPureAndThatImplementsAnInterfaceMakesMethodImpure()
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

public abstract class Base
{
    public abstract int Method();
}

public class Middle : Base, IInterface
{
    public override int Method() => 2;
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
        public void DownCastingFromObjectToTypeWhoseSubTypeOverridesAnAbstractMethodWithAMethodThatIsPureAndTypeImplementsAnInterfaceMakesMethodImpure()
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

public abstract class Base
{
    public abstract int Method();
}

public class Middle : Base
{
    public override int Method() => 2;
}

public class Derived : Middle, IInterface
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
        public void DownCastingFromObjectToTypeWhoseSubTypeOverridesAnAbstractMethodThatImplementsAnInterfaceWithASealedMethodThatIsPureKeepsMethodPure()
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

public abstract class Base : IInterface
{
    public abstract int Method();
}

public class Middle : Base
{
    public sealed override int Method() => 2;
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
        public void DownCastingFromObjectToTypeWhoseSubTypeOverridesAnAbstractMethodWithASealedMethodThatIsPureAndThatImplementsAnInterfaceKeepsMethodPure()
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

public abstract class Base
{
    public abstract int Method();
}

public class Middle : Base, IInterface
{
    public sealed override int Method() => 2;
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
        public void DownCastingFromObjectToTypeWhoseSubTypeOverridesAnAbstractMethodWithASealedMethodThatIsPureAndTypeImplementsAnInterfaceKeepsMethodPure()
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

public abstract class Base
{
    public abstract int Method();
}

public class Middle : Base
{
    public sealed override int Method() => 2;
}

public class Derived : Middle, IInterface
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
        public void DownCastingFromObjectToTypeWhoseSubTypeOverridesAnAbstractMethodThatImplementsAnInterfaceWithASealedMethodThatIsImpureKeepsMethodPure()
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

public abstract class Base : IInterface
{
    public abstract int Method();
}

public class Middle : Base
{
    static int state = 0;
    public sealed override int Method() => state++;
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
        public void DownCastingFromObjectToTypeWhoseSubTypeOverridesAnAbstractMethodWithASealedMethodThatIsImpureAndThatImplementsAnInterfaceKeepsMethodPure()
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

public abstract class Base
{
    public abstract int Method();
}

public class Middle : Base, IInterface
{
    static int state = 0;
    public sealed override int Method() => state++;
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
        public void DownCastingFromObjectToTypeWhoseSubTypeOverridesAnAbstractMethodWithASealedMethodThatIsImpureAndTypeImplementsAnInterfaceKeepsMethodPure()
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

public abstract class Base
{
    public abstract int Method();
}

public class Middle : Base
{
    static int state = 0;
    public sealed override int Method() => state++;
}

public class Derived : Middle, IInterface
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

    }
}
