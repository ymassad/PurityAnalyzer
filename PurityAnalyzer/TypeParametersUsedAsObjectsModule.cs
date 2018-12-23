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

        public static bool DoesMethodUseTAsObject_IncludingStaticConstructorsIfRelevant(
            IMethodSymbol method,
            SemanticModel semanticModel,
            ITypeParameterSymbol typeParameter,
            IMethodSymbol[] relevantObjectMethods,
            KnownSymbols knownSymbols,
            RecursiveStateForNotUsedAsObject recursiveStateForNotUsedAsObject)
        {
            IEnumerable<INamedTypeSymbol> GetAllContainingTypes()
            {
                var containingType = method.ContainingType;

                while (containingType != null)
                {
                    yield return containingType;

                    containingType = containingType.ContainingType;
                }
            }

            if (method.IsStatic || method.MethodKind == MethodKind.Constructor)
            {
                var containingTypes = GetAllContainingTypes().ToList();

                var staticConstructors = containingTypes.SelectMany(x => x.StaticConstructors).ToList();

                if (staticConstructors
                    .Any(c =>
                        DoesMethodUseTAsObject(c, semanticModel, typeParameter, relevantObjectMethods, knownSymbols, recursiveStateForNotUsedAsObject)))
                    return true;
            }

            return DoesMethodUseTAsObject(method, semanticModel, typeParameter, relevantObjectMethods, knownSymbols, recursiveStateForNotUsedAsObject);
        }

        private static bool DoesMethodUseTAsObject(IMethodSymbol method, SemanticModel semanticModel,
            ITypeParameterSymbol typeParameter, IMethodSymbol[] relevantObjectMethods, KnownSymbols knownSymbols,
            RecursiveStateForNotUsedAsObject recursiveStateForNotUsedAsObject)
        {
            if (recursiveStateForNotUsedAsObject.ItemsInStack.Contains((method, typeParameter)))
                return false;

            recursiveStateForNotUsedAsObject = recursiveStateForNotUsedAsObject.Add(method, typeParameter);

            //TODO: for class-level type parameters, a method should be able to declare that it does not use T via a method level attribute even if T does not have NotUsedAsObject

            if (typeParameter.GetAttributes().Any(x => Utils.IsNotUsedAsObjectAttribute(x.AttributeClass.Name)))
                return false;


            if (method.IsInCode())
            {
                var location = method.Locations.First();

                var locationSourceTree = location.SourceTree;

                var methodSyntax = locationSourceTree.GetRoot().FindNode(location.SourceSpan);

                return GetNodesWhereTIsUsedAsObject(methodSyntax, semanticModel, relevantObjectMethods, typeParameter,
                    knownSymbols, recursiveStateForNotUsedAsObject).Any();
            }

            if (typeParameter.DeclaringMethod != null)
            {
                if (knownSymbols.KnownNotUsedAsObjectMethodTypeParameters.TryGetValue(
                        Utils.GetFullMetaDataName(typeParameter.DeclaringMethod.ContainingType),
                        out var methods) &&
                    methods.Keys.FirstOrNoValue(x => x.Matches(typeParameter.DeclaringMethod))
                        .HasValueAnd(x => methods[x].Contains(typeParameter.Name)))
                {
                    return false;
                }
            }
            else
            {
                if (knownSymbols.KnownNotUsedAsObjectClassTypeParameters.TryGetValue(
                        Utils.GetFullMetaDataName(typeParameter.DeclaringType),
                        out var methods) &&
                    methods.Contains(typeParameter.Name))
                {
                    return false;
                }
            }

            return true;
        }

        public static IEnumerable<SyntaxNode> GetNodesWhereTIsUsedAsObject(SyntaxNode scope,
            SemanticModel semanticModel,
            IMethodSymbol[] relevantObjectMethods,
            ITypeParameterSymbol typeParameterSymbol,
            KnownSymbols knownSymbols,
            RecursiveStateForNotUsedAsObject recursiveStateForNotUsedAsObject)
        {
            var objectType = semanticModel.Compilation.ObjectType;

            var invocationOperations = scope.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Select(x => semanticModel.GetOperation(x))
                .OfType<IInvocationOperation>()
                .Where(x => x.TargetMethod.ContainingType.TypeKind != TypeKind.Delegate)
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
                Utils.GetConversions(scope, semanticModel);


            var conversionsFromTToObject =
                conversions.Where(x => x.@from.Equals(typeParameterSymbol) &&
                                       x.to.Equals(objectType))
                    .Select(x => x.node)
                    .ToList();

            foreach (var conv in conversionsFromTToObject)
            {
                yield return conv;
            }



            var constructionOperations =
                scope.DescendantNodes()
                    .OfType<ObjectCreationExpressionSyntax>()
                    .Select(x => semanticModel.GetOperation(x))
                    .OfType<IObjectCreationOperation>()
                    .Where(x => x.Constructor.ContainingType.TypeKind != TypeKind.Delegate)
                    .ToList();



            var invokedMethodsAndConstructorsWithRelevantTypeParameters =
                invocationOperations
                    .Select(x => (Method: x.TargetMethod, x.Syntax))
                    .Concat(constructionOperations.Select(x => (Method: x.Constructor, x.Syntax)))
                    .Select(x => (
                        node: x.Syntax,
                        method: x.Method,
                        typeParamsAndArgs:
                            GetTypeParametersAndMatchingArguments(x.Method)
                                .Where(p =>
                                    p.argument.Equals(typeParameterSymbol)).ToList()))
                    .ToList();

            foreach (var invokedMethodOrConstructor in invokedMethodsAndConstructorsWithRelevantTypeParameters)
            {
                foreach (var tp in invokedMethodOrConstructor.typeParamsAndArgs)
                {
                    if (DoesMethodUseTAsObject_IncludingStaticConstructorsIfRelevant(
                        invokedMethodOrConstructor.method,
                        semanticModel,
                        tp.typeParameter,
                        relevantObjectMethods,
                        knownSymbols,
                        recursiveStateForNotUsedAsObject))
                    {
                        yield return invokedMethodOrConstructor.node;
                        break;
                    }
                }
            }
        }

        public static IEnumerable<(ITypeParameterSymbol typeParameter, ITypeSymbol argument)> GetTypeParametersAndMatchingArguments(IMethodSymbol method)
        {
            IEnumerable<(ITypeParameterSymbol typeParameter, ITypeSymbol argument)>
            GetTypeParametersAndMatchingArgumentsForClass(INamedTypeSymbol @class)
            {
                for (int i = 0; i < @class.TypeParameters.Length; i++)
                {
                    yield return (@class.TypeParameters[i], @class.TypeArguments[i]);
                }

                if (@class.ContainingType != null)
                {
                    foreach (var item in GetTypeParametersAndMatchingArgumentsForClass(@class.ContainingType))
                    {
                        yield return item;
                    }
                }
            }

            for (int i = 0; i < method.TypeParameters.Length; i++)
            {
                yield return (method.TypeParameters[i], method.TypeArguments[i]);
            }


            foreach (var item in GetTypeParametersAndMatchingArgumentsForClass(method.ContainingType))
            {
                yield return item;
            }
        }
    }
}
