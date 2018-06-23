using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureExceptReadLocallyAttribute
{
    [TestFixture]
    public class IsPureExceptReadLocallyAttributeOnProperty
    {
        [Test]
        public void PropertyGetterThatReadsAConstantFieldIsPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class Class1
{
    const int c = 1;

    [IsPureExceptReadLocally]
    public int DoSomething => c;    
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void IsPureExceptReadLocallyAttributeCannotBeAppliedOnStaticProperties()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class Class1
{
    [IsPureExceptReadLocally]
    public static int DoSomething => 1;
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();

        }

        [Test]
        public void PropertyGetterThatReadsAReadOnlyFieldIsPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class Class1
{
    readonly int c = 1;

    [IsPureExceptReadLocally]
    public int DoSomething => c;
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void PropertyGetterThatReadsALocalReadWriteFieldIsPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class Class1
{
    int c = 1;

    [IsPureExceptReadLocally]
    public int DoSomething => c;
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().Be(0);
        }

        [Test]
        public void PropertyGetterThatWritesALocalReadWriteFieldIsNotPureExceptReadLocally()
        {
            string code = @"
using System;

public class IsPureExceptReadLocallyAttribute : Attribute
{
}

public class Class1
{
    int c = 1;

    [IsPureExceptReadLocally]
    public int DoSomething
    {
        get
        {
            c = 2;
            return 1;
        }
    }
}";

            var dignostics = Utilities.RunPurityAnalyzer(code);
            dignostics.Length.Should().BePositive();
        }
        
    }
}
