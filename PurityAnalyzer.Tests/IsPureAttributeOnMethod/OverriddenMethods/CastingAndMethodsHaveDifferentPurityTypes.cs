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
    public class CastingAndMethodsHaveDifferentPurityTypes
    {
        [Test]
        public void UpCastingIsAllowedWhereTargetMethodIsPureExceptLocallyAndSourceMethodIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    int state = 0;
    public virtual int Method() => state++;
}

public class Derived : Base
{
    public override int Method() => 1;
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        Base x = source;

        return 1;
    }

    public static readonly Derived source = new Derived();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void UpCastingIsNotAllowedWhereTargetMethodIsPureAndSourceMethodIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    public virtual int Method() => 1;
}

public class Derived : Base
{
    int state = 0;
    public override int Method() => state++;
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        Base x = source;

        return 1;
    }

    public static readonly Derived source = new Derived();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void DownCastingIsNotAllowedWhereTargetMethodIsPureAndSourceMethodIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    int state = 0;
    public virtual int Method() => state++;
}

public class Derived : Base
{
    public override int Method() => 1;
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        Derived x = (Derived) source;

        return 1;
    }

    public static readonly Base source = new Base();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void DownCastingIsAllowedWhereTargetMethodIsPureExceptLocallyAndSourceMethodIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    public virtual int Method() => 1;
}

public class Derived : Base
{
    int state = 0;
    public override int Method() => state++;
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        Derived x = (Derived) source;

        return 1;
    }

    public static readonly Base source = new Base();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void UpCastingIsAllowedWhereTargetMethodIsPureExceptLocallyAndSourceMethodIsPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    int state = 0;
    public virtual int Method() => state++;
}

public class Derived : Base
{
    int state = 0;
    public override int Method() => state;
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        Base x = source;

        return 1;
    }

    public static readonly Derived source = new Derived();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void UpCastingIsNotAllowedWhereTargetMethodIsPureExceptReadLocallyAndSourceMethodIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    int state = 0;
    public virtual int Method() => state;
}

public class Derived : Base
{
    int state = 0;
    public override int Method() => state++;
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        Base x = source;

        return 1;
    }

    public static readonly Derived source = new Derived();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void DownCastingIsNotAllowedWhereTargetMethodIsPureExceptReadLocallyAndSourceMethodIsPureExceptLocally()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    int state = 0;
    public virtual int Method() => state++;
}

public class Derived : Base
{
    int state = 0;
    public override int Method() => state;
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        Derived x = (Derived) source;

        return 1;
    }

    public static readonly Base source = new Base();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void DownCastingIsAllowedWhereTargetMethodIsPureExceptLocallyAndSourceMethodIsPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    int state = 0;
    public virtual int Method() => state;
}

public class Derived : Base
{
    int state = 0;
    public override int Method() => state++;
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        Derived x = (Derived) source;

        return 1;
    }

    public static readonly Base source = new Base();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void UpCastingIsAllowedWhereTargetMethodIsPureExceptReadLocallyAndSourceMethodIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    int state = 0;
    public virtual int Method() => state;
}

public class Derived : Base
{
    public override int Method() => 1;
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        Base x = source;

        return 1;
    }

    public static readonly Derived source = new Derived();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

        [Test]
        public void UpCastingIsNotAllowedWhereTargetMethodIsPureAndSourceMethodIsPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    public virtual int Method() => 1;
}

public class Derived : Base
{
    int state = 0;
    public override int Method() => state;
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        Base x = source;

        return 1;
    }

    public static readonly Derived source = new Derived();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void DownCastingIsNotAllowedWhereTargetMethodIsPureAndSourceMethodIsPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    int state = 0;
    public virtual int Method() => state;
}

public class Derived : Base
{
    public override int Method() => 1;
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        Derived x = (Derived) source;

        return 1;
    }

    public static readonly Base source = new Base();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }

        [Test]
        public void DownCastingIsAllowedWhereTargetMethodIsPureExceptReadLocallyAndSourceMethodIsPure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public class Base
{
    public virtual int Method() => 1;
}

public class Derived : Base
{
    int state = 0;
    public override int Method() => state;
}

public static class Module1
{
    [IsPure]
    public static int DoSomething()
    {
        Derived x = (Derived) source;

        return 1;
    }

    public static readonly Base source = new Base();
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);

        }

    }
}
