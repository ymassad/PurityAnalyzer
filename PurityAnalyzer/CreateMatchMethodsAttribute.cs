using System;

namespace PurityAnalyzer
{
    public class CreateMatchMethodsAttribute : Attribute
    {
        public Type[] Types { get; }

        public CreateMatchMethodsAttribute(params Type[] types)
        {
            Types = types;
        }
    }
}