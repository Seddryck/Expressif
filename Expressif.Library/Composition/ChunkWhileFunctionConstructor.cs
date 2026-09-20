using Expressif.Bindings;
using Expressif.Semantics;

namespace Expressif.Library.Composition;

internal sealed class ChunkWhileFunctionConstructor : IFunctionConstructor<Expressif.Library.Array.Partitioning.ChunkWhile>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
    {
        if (function.Parameters.Length != 1
            || !FunctionConstructorSupport.TryGetOpenExpression(function.Parameters[0], out var open))
        {
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);
        }

        var members = open.Expression.Members.ToArray();
        IFunction operation;
        if (LegacyTupleBindingRules.IsCandidate("chunk-while", open.Expression)
            && AdjacentFunctionConstructor.TryBuildBinaryCallable(
                members[0].Name, context, constructionContext, out var callable))
        {
            var functions = new List<IFunction>
            {
                constructionContext.CreateFunction(
                    new Bindings.Function("tuple-at", [new LiteralParameter("1")]), context),
                callable,
            };
            if (members.Length > 1)
            {
                functions.Add(constructionContext.CreateOpenExpression(
                    new OpenExpression(members.Skip(1)), context));
            }
            operation = new ChainFunction(functions);
        }
        else
        {
            operation = constructionContext.CreateOpenExpression(open.Expression, context);
        }

        return new Expressif.Library.Array.Partitioning.ChunkWhile(() => new LexicallyBoundContextFunction(operation));
    }
}
