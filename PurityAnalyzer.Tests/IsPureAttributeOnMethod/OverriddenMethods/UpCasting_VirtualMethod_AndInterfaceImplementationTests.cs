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
    public class UpCasting_VirtualMethod_AndInterfaceImplementationTests
    {
        [Test]
        public void UpCastingFromObjectToTypeWithVirtualPureMethodThatImplementsAnInterfaceMakesMethodImpure()
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

public class Base : IInterface
{
    public virtual int Method() => 1;
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
        public void UpCastingFromObjectToTypeWithVirtualImpureMethodThatImplementsAnInterfaceKeepsMethodPure()
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

public class Base : IInterface
{
    static int state = 0;
    public virtual int Method() => state++;
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
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void UpCastingFromObjectToTypeThatOverridesAPureVirtualMethodThatImplementsAnInterfaceWithAMethodThatIsImpureKeepsMethodPure()
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

public class Base : IInterface
{
    public virtual int Method() => 1;
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
        public void UpCastingFromObjectToTypeThatOverridesAPureVirtualMethodWithAMethodThatIsImpureAndThatImplementsAnInterfaceKeepsMethodPure()
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

public class Base
{
    public virtual int Method() => 1;
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
        public void UpCastingFromObjectToTypeThatOverridesAPureVirtualMethodThatImplementsAnInterfaceWithAMethodThatIsPureMakesMethodImpure()
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

public class Base : IInterface
{
    public virtual int Method() => 1;
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
        public void UpCastingFromObjectToTypeThatOverridesAPureVirtualMethodWithAMethodThatIsPureAndThatImplementsAnInterfaceMakesMethodImpure()
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

public class Base
{
    public virtual int Method() => 1;
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
        public void UpCastingFromObjectToTypeThatOverridesAnImpureVirtualMethodThatImplementsAnInterfaceWithAMethodThatIsPureMakesMethodImpure()
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

public class Base : IInterface
{
    static int state = 0;
    public virtual int Method() => state++;
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
        public void UpCastingFromObjectToTypeThatOverridesAnImpureVirtualMethodWithAMethodThatIsPureAndThatImplementsAnInterfaceMakesMethodImpure()
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

public class Base
{
    static int state = 0;
    public virtual int Method() => state++;
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
        public void UpCastingFromObjectToSealedTypeThatOverridesAPureVirtualMethodThatImplementsAnInterfaceWithAMethodThatIsPureKeepsMethodPure()
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

public class Base : IInterface
{
    public virtual int Method() => 1;
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
        public void UpCastingFromObjectToSealedTypeThatOverridesAPureVirtualMethodWithAMethodThatIsPureAndThatImplementsAnInterfaceKeepsMethodPure()
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

public class Base
{
    public virtual int Method() => 1;
}

public sealed class Derived : Base , IInterface
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
        public void UpCastingFromObjectToTypeThatOverridesAPureVirtualMethodThatImplementsAnInterfaceWithASealedMethodThatIsPureKeepsMethodPure()
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

public class Base : IInterface
{
    public virtual int Method() => 1;
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
        public void UpCastingFromObjectToTypeThatOverridesAPureVirtualMethodWithASealedMethodThatIsPureAndThatImplementsAnInterfaceKeepsMethodPure()
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

public class Base
{
    public virtual int Method() => 1;
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
        public void UpCastingFromObjectToTypeThatOverridesAPureVirtualMethodThatImplementsAnInterfaceWithASealedMethodThatIsImpureKeepsMethodPure()
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

public class Base : IInterface
{
    public virtual int Method() => 1;
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
        public void UpCastingFromObjectToTypeThatOverridesAPureVirtualMethodWithASealedMethodThatIsImpureAndThatImplementsAnInterfaceKeepsMethodPure()
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

public class Base
{
    public virtual int Method() => 1;
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
        public void UpCastingFromObjectToTypeThatOverridesAnImpureVirtualMethodThatImplementsAnInterfaceWithASealedMethodThatIsPureKeepsMethodPure()
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

public class Base : IInterface
{
    static int state = 0;
    public virtual int Method() => state++;
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
        public void UpCastingFromObjectToTypeThatOverridesAnImpureVirtualMethodWithASealedMethodThatIsPureAndThatImplementsAnInterfaceKeepsMethodPure()
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

public class Base
{
    static int state = 0;
    public virtual int Method() => state++;
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
        public void UpCastingFromObjectToTypeWhoseSubTypeOverridesAPureVirtualMethodThatImplementsAnInterfaceWithAMethodThatIsImpureKeepsMethodPure()
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

public class Base : IInterface
{
    public virtual int Method() => 1;
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
        public void UpCastingFromObjectToTypeWhoseSubTypeOverridesAPureVirtualMethodWithAMethodThatIsImpureAndThatImplementsAnInterfaceKeepsMethodPure()
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

public class Base
{
    public virtual int Method() => 1;
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
        public void UpCastingFromObjectToTypeWhoseSubTypeOverridesAPureVirtualMethodWithAMethodThatIsImpureAndTypeImplementsAnInterfaceKeepsMethodPure()
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

public class Base
{
    public virtual int Method() => 1;
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
        public void UpCastingFromObjectToTypeWhoseSubTypeOverridesAPureVirtualMethodThatImplementsAnInterfaceWithAMethodThatIsPureMakesMethodImpure()
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

public class Base : IInterface
{
    public virtual int Method() => 1;
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
        public void UpCastingFromObjectToTypeWhoseSubTypeOverridesAPureVirtualMethodWithAMethodThatIsPureAndThatImplementsAnInterfaceMakesMethodImpure()
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

public class Base
{
    public virtual int Method() => 1;
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
        public void UpCastingFromObjectToTypeWhoseSubTypeOverridesAPureVirtualMethodWithAMethodThatIsPureAndTypeImplementsAnInterfaceMakesMethodImpure()
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

public class Base
{
    public virtual int Method() => 1;
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
        public void UpCastingFromObjectToTypeWhoseSubTypeOverridesAnImpureVirtualMethodThatImplementsAnInterfaceWithAMethodThatIsPureMakesMethodImpure()
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

public class Base : IInterface
{
    static int state = 0;
    public virtual int Method() => state++;
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
        public void UpCastingFromObjectToTypeWhoseSubTypeOverridesAnImpureVirtualMethodWithAMethodThatIsPureAndThatImplementsAnInterfaceMakesMethodImpure()
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

public class Base
{
    static int state = 0;
    public virtual int Method() => state++;
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
        public void UpCastingFromObjectToTypeWhoseSubTypeOverridesAnImpureVirtualMethodWithAMethodThatIsPureAndTypeImplementsAnInterfaceMakesMethodImpure()
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

public class Base
{
    static int state = 0;
    public virtual int Method() => state++;
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
        public void UpCastingFromObjectToTypeWhoseSubTypeOverridesAPureVirtualMethodThatImplementsAnInterfaceWithASealedMethodThatIsPureKeepsMethodPure()
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

public class Base : IInterface
{
    public virtual int Method() => 1;
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
        public void UpCastingFromObjectToTypeWhoseSubTypeOverridesAPureVirtualMethodWithASealedMethodThatIsPureAndThatImplementsAnInterfaceKeepsMethodPure()
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

public class Base
{
    public virtual int Method() => 1;
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
        public void UpCastingFromObjectToTypeWhoseSubTypeOverridesAPureVirtualMethodWithASealedMethodThatIsPureAndTypeImplementsAnInterfaceKeepsMethodPure()
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

public class Base
{
    public virtual int Method() => 1;
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
        public void UpCastingFromObjectToTypeWhoseSubTypeOverridesAPureVirtualMethodThatImplementsAnInterfaceWithASealedMethodThatIsImpureKeepsMethodPure()
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

public class Base : IInterface
{
    public virtual int Method() => 1;
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
        public void UpCastingFromObjectToTypeWhoseSubTypeOverridesAPureVirtualMethodWithASealedMethodThatIsImpureAndThatImplementsAnInterfaceKeepsMethodPure()
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

public class Base
{
    public virtual int Method() => 1;
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
        public void UpCastingFromObjectToTypeWhoseSubTypeOverridesAPureVirtualMethodWithASealedMethodThatIsImpureAndTypeImplementsAnInterfaceKeepsMethodPure()
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

public class Base
{
    public virtual int Method() => 1;
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


        [Test]
        public void UpCastingFromObjectToTypeWhoseSubTypeOverridesAnImpureVirtualMethodThatImplementsAnInterfaceWithASealedMethodThatIsPureKeepsMethodPure()
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

public class Base : IInterface
{
    static int state = 0;
    public virtual int Method() => state++;
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
        public void UpCastingFromObjectToTypeWhoseSubTypeOverridesAnImpureVirtualMethodWithASealedMethodThatIsPureAndThatImplementsAnInterfaceKeepsMethodPure()
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

public class Base
{
    static int state = 0;
    public virtual int Method() => state++;
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
        public void UpCastingFromObjectToTypeWhoseSubTypeOverridesAnImpureVirtualMethodWithASealedMethodThatIsPureAndTypeImplementsAnInterfaceKeepsMethodPure()
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

public class Base
{
    static int state = 0;
    public virtual int Method() => state++;
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

    }
}
