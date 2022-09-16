using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace PurityAnalyzer.Tests.IsPureAttributeOnMethod
{
    [TestFixture]
    public class EventTests
    {
        [Test]
        public void MethodThatRaisesAnEventIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    public static event Action myEvent;
    
    [IsPure]
    public static void DoSomething()
    {
        myEvent();
    }
}";

            Utilities.AssertImpure(code);

        }

        [Test]
        public void MethodThatSubscribesToAnEventIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    public static event Action myEvent;
    
    [IsPure]
    public static void DoSomething()
    {
        myEvent += () => {};
    }
}";

            Utilities.AssertImpure(code);

        }

        [Test]
        public void MethodThatUnsubscribesFromAnEventIsImpure()
        {
            string code = @"
using System;

public class IsPureAttribute : Attribute
{
}

public static class Module1
{
    public static event Action myEvent;
    
    [IsPure]
    public static void DoSomething()
    {
        myEvent -= () => {};
    }
}";

            Utilities.AssertImpure(code);

        }
    }
}
