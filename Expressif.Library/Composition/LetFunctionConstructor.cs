using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal sealed class LetFunctionConstructor : IFunctionConstructor<Expressif.Library.Flow.Let>
{
    public IFunction Construct(
        Bindings.Function function,
        IContext context,
        IFunctionConstructionContext constructionContext)
    {
        if (function.Parameters is not [LetDefinitionParameter definition])
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);
        var bindings = definition.Bindings.Select(binding => new Expressif.Library.Flow.NamedExpressionEvaluator(
            binding.Name,
            constructionContext.CreateValueEvaluator(binding.Value, context))).ToArray();
        return new Expressif.Library.Flow.Let(() => bindings);
    }
}
