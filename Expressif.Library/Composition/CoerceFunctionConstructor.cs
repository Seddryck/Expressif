using Expressif.Bindings;
using Expressif.Library.Special;

namespace Expressif.Library.Composition;

internal sealed class CoerceFunctionConstructor : IFunctionConstructor<Coerce>
{
    public IFunction Construct(
        Bindings.Function function,
        IContext context,
        IFunctionConstructionContext constructionContext)
    {
        if (function.Parameters.All(parameter => parameter is PositionalCoercionParameter))
        {
            return new Coerce(function.Parameters
                .Cast<PositionalCoercionParameter>()
                .Select(parameter => parameter.TargetType)
                .ToArray());
        }

        var mappings = function.Parameters.Select(parameter => parameter switch
        {
            FieldCoercionParameter field => new CoercionMapping(
                new FieldCoercionSelector(field.Field),
                field.TargetType),
            TupleCoercionParameter tuple => new CoercionMapping(
                new TupleCoercionSelector(tuple.Position),
                tuple.TargetType),
            _ => throw new InvalidOperationException(
                $"Unsupported bound coercion specification '{parameter.GetType().Name}'."),
        }).ToArray();
        return new Coerce(mappings);
    }
}
