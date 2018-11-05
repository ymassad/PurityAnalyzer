using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace PurityAnalyzer
{
    public static class TypeParametersUsedAsObjectsModule
    {
        public static IMethodSymbol[] GetObjectMethodsRelevantToCastingFromGenericTypeParameters(SemanticModel semanticModel)
        {
            var objectType = semanticModel.Compilation.ObjectType;

            var toStringMethod = objectType.GetMembers("ToString")
                .OfType<IMethodSymbol>().Single(x => x.Parameters.IsEmpty);

            var getHashCodeMethod = objectType.GetMembers("GetHashCode")
                .OfType<IMethodSymbol>().Single(x => x.Parameters.IsEmpty);

            var equalsMethod = objectType.GetMembers("Equals")
                .OfType<IMethodSymbol>().Single(x => x.Parameters.Length == 1 && x.Parameters.First().Type.Equals(objectType));

            var relevantObjectMethods = new[] {toStringMethod, getHashCodeMethod, equalsMethod};
            return relevantObjectMethods;
        }

        public static bool IsTIsUsedAsObject(
            IMethodSymbol method,
            SemanticModel semanticModel,
            ITypeParameterSymbol typeParameter,
            IMethodSymbol[] relevantObjectMethods)
        {
            if (method.IsInCode())
            {
                var location = method.Locations.First();

                var locationSourceTree = location.SourceTree;

                var methodSyntax = (MethodDeclarationSyntax) locationSourceTree.GetRoot().FindNode(location.SourceSpan);

                return Enumerable.Any<SyntaxNode>(GetNodesWhereTIsUsedAsObject(methodSyntax, semanticModel, relevantObjectMethods, typeParameter));

            }

            return !typeParameter.GetAttributes().Any(x => Utils.IsNotUsedAsObjectAttribute(x.AttributeClass.Name));
        }

        public static IEnumerable<SyntaxNode> GetNodesWhereTIsUsedAsObject(
            MethodDeclarationSyntax method,
            SemanticModel semanticModel,
            IMethodSymbol[] relevantObjectMethods,
            ITypeParameterSymbol typeParameterSymbol)
        {
            var objectType = semanticModel.Compilation.ObjectType;

            var invocationOperations = method.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Select(x => semanticModel.GetOperation(x))
                .OfType<IInvocationOperation>()
                .ToList();

            var invocationsOfObjectMethodsOnExpressionsOfTypeT =
                invocationOperations
                    .Where(x => x.Instance?.Type.Equals(typeParameterSymbol) ?? false)
                    .Where(x => relevantObjectMethods.Contains(x.TargetMethod))
                    .ToList();

            foreach (var inv in invocationsOfObjectMethodsOnExpressionsOfTypeT)
            {
                yield return inv.Syntax;
            }


            var conversions =
                Utils.GetConversions(method, semanticModel);


            var conversionsFromTToObject =
                conversions.Where(x => x.@from.Equals(typeParameterSymbol) &&
                                       x.to.Equals(objectType))
                    .Select(x => x.node)
                    .ToList();

            foreach (var conv in conversionsFromTToObject)
            {
                yield return conv;
            }

            var invokedMethodWithRelevantTypeParameters =
                invocationOperations
                    .Select(x => (node: x.Syntax, method: x.TargetMethod, typeParams: x.TargetMethod.TypeParameters
                        .Where((_, i) =>
                            x.TargetMethod.TypeArguments[i].Equals(typeParameterSymbol)).ToList()))
                    .ToList();

            foreach (var invokedMethod in invokedMethodWithRelevantTypeParameters)
            {
                foreach (var tp in invokedMethod.typeParams)
                {
                    if (IsTIsUsedAsObject(invokedMethod.method, semanticModel, tp, relevantObjectMethods))
                    {
                        yield return invokedMethod.node;
                        break;
                    }
                }
            }
        }
    }
}
