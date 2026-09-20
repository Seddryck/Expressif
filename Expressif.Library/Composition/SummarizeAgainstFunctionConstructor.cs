using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal sealed class SummarizeAgainstFunctionConstructor : IFunctionConstructor<Expressif.Library.Grouping.SummarizeAgainst>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
    {
        var bound = ParameterArgumentBinder.Bind(typeof(Expressif.Library.Grouping.SummarizeAgainst), function.Arguments).Parameters;
        if (!FunctionConstructorSupport.TryGetOpenExpression(bound[2], out var operation))
        {
            throw new ArgumentException(
                $"The function named '{function.Name}' expects a combine expression.",
                nameof(function));
        }

        return new Expressif.Library.Grouping.SummarizeAgainst(
            constructionContext.CreateAccumulatorProvider(bound[0], context),
            constructionContext.CreateAccumulatorProvider(bound[1], context),
            constructionContext.CreateTransformationProvider(operation, context));
    }
}
