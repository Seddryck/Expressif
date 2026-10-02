using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal sealed class PivotFunctionConstructor : IFunctionConstructor<Expressif.Library.Array.Grouping.Pivot>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
    {
        if (function.Arguments.Any(argument => argument.IsSpread))
            throw new SpreadArgumentException("Spread arguments are not supported by pivot.");

        var bound = ParameterArgumentBinder.Bind(typeof(Expressif.Library.Array.Grouping.Pivot), function.Arguments).Parameters;
        var dimensions = bound[0] is TupleParameter tuple
            ? tuple.Elements.All(element => !element.IsSpread) ? tuple.Values : []
            : [bound[0]];
        if (dimensions.Length == 0)
        {
            throw new BindingException(
                "The pivot row must contain at least one direct field selector; spread and unnamed dimensions are not supported.");
        }

        NamedFieldSelector BuildRow(IParameter parameter)
        {
            if (!ExpressionShapeNormalizer.TryGetDirectFieldName(parameter, out var name))
            {
                throw new BindingException(
                    "Each pivot row dimension must be a direct field selector such as .country; computed or unnamed row expressions are not supported.");
            }
            return new NamedFieldSelector(name, BuildExpression(parameter));
        }

        Func<object?, object?> BuildExpression(IParameter parameter)
        {
            var evaluator = new DelegatedFunction(
                constructionContext.CreateValueEvaluator(parameter, context));
            return value => FunctionConstructorSupport.EvaluateNested(evaluator, value);
        }

        var rows = dimensions.Select(BuildRow).ToArray();
        if (rows.Select(row => row.Name).Distinct(StringComparer.Ordinal).Count() != rows.Length)
            throw new BindingException("Duplicate pivot row field names are not supported.");
        return new Expressif.Library.Array.Grouping.Pivot(rows, BuildExpression(bound[1]), BuildExpression(bound[2]));
    }
}
