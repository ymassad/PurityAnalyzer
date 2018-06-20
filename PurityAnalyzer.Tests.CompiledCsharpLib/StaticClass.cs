using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurityAnalyzer.Tests.CompiledCsharpLib
{
    public static class StaticClass
    {
        [IsPure]
        public static string PureMethod()
        {
            return "";
        }

        private static int state = 0;

        public static string ImpureMethod()
        {
            return (state++).ToString();
        }

        [ReturnsNewObject]
        public static MutableDto1 CreateNew() => new MutableDto1();

        public static MutableDto1 ReturnExisting() => dto1;

        private static MutableDto1 dto1 = new MutableDto1();
    }
}
