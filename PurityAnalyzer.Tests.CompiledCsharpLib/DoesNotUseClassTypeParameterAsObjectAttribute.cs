using System;

namespace PurityAnalyzer.Tests.CompiledCsharpLib
{
    public class DoesNotUseClassTypeParameterAsObjectAttribute : Attribute
    {
        private readonly string parameterName;

        public DoesNotUseClassTypeParameterAsObjectAttribute(string parameterName) => this.parameterName = parameterName;
    }
}