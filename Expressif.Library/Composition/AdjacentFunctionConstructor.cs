using Expressif.Bindings;
using Expressif.Semantics;

namespace Expressif.Library.Composition;

internal sealed class AdjacentFunctionConstructor : IFunctionConstructor<Expressif.Library.Array.Sequencing.Adjacent>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
    {
        if (function.Arguments.Any(argument => argument.Name is not null))
        {
            function = new Bindings.Function(
                function.Name,
                ParameterArgumentBinder.Bind(typeof(Expressif.Library.Array.Sequencing.Adjacent), function.Arguments).Parameters);
        }
        if (function.Parameters.Length != 1
            || !FunctionConstructorSupport.TryGetOpenExpression(function.Parameters[0], out var open))
        {
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);
        }

        var members = open.Expression.Members.ToArray();
        IFunction operation;
        if (LegacyTupleBindingRules.IsCandidate("adjacent", open.Expression)
            && TryBuildBinaryCallable(members[0].Name, context, constructionContext, out var callable))
        {
            operation = new ChainFunction([
                constructionContext.CreateFunction(
                    new Bindings.Function("tuple-at", [new LiteralParameter("1")]), context),
                callable,
            ]);
        }
        else
        {
            operation = constructionContext.CreateOpenExpression(open.Expression, context);
        }

        return new Expressif.Library.Array.Sequencing.Adjacent(() => new LexicallyBoundContextFunction(operation));
    }

    internal static bool TryBuildBinaryCallable(
        string name,
        IContext context,
        IFunctionConstructionContext constructionContext,
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out IFunction? callable)
    {
        if (!constructionContext.TryResolveImplementation(name, out var functionType)
            || !LegacyTupleBindingRules.HasBinarySignature(functionType))
        {
            callable = null;
            return false;
        }

        callable = constructionContext.CreateFunction(
            new Bindings.Function(name, [new TupleProjectionParameter(0)]), context);
        return true;
    }
}
