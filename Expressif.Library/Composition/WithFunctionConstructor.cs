using Expressif.Bindings;
using Expressif.Library.Numeric;
using Expressif.Library.Record;

namespace Expressif.Library.Composition;

internal sealed class WithFunctionConstructor : IFunctionConstructor<With>
{
    public IFunction Construct(
        Bindings.Function function,
        IContext context,
        IFunctionConstructionContext constructionContext)
    {
        if (function.Parameters is not [WithDefinitionParameter definition])
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);

        var projections = definition.Projections
            .Select(projection => RecordEntryEvaluator.Named(
                projection.Name,
                constructionContext.CreateValueEvaluator(projection.Value, context)))
            .ToArray();
        var body = constructionContext.CreateValueEvaluator(definition.Body, context);
        return new With(
            () => projections,
            body,
            definition.Body is OpenExpressionParameter { Expression.InputBinding: not null });
    }
}
