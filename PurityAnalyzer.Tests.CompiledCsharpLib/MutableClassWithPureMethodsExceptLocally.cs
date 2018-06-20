using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurityAnalyzer.Tests.CompiledCsharpLib
{
    public class MutableClassWithPureMethodsExceptLocally
    {
        [IsPure]
        public MutableClassWithPureMethodsExceptLocally()
        {
            
        }

        public int state = 0;

        [IsPureExceptLocally]
        public int Increment() => state++;
    }
}
