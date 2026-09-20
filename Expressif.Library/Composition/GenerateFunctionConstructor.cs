using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal sealed class GenerateFunctionConstructor : IFunctionConstructor<Expressif.Library.Array.Generate>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
    {
        if (function.Arguments.Any(argument => argument.Name is not null))
        {
            function = new Bindings.Function(
                function.Name,
                ParameterArgumentBinder.Bind(typeof(Expressif.Library.Array.Generate), function.Arguments).Parameters);
        }
        if (function.Parameters.Length is < 2 or > 3)
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);

        var condition = constructionContext.CreatePredicateProvider(
            function.Parameters[0], context, function.Name);
        if (!FunctionConstructorSupport.TryGetOpenExpression(function.Parameters[1], out var next))
        {
            throw new ArgumentException(
                $"The function named '{function.Name}' expects parameter 'next' to be an open expression.",
                nameof(function));
        }

        Func<IFunction>? result = null;
        if (function.Parameters.Length == 3)
        {
            if (!FunctionConstructorSupport.TryGetOpenExpression(function.Parameters[2], out var projection))
            {
                throw new ArgumentException(
                    $"The function named '{function.Name}' expects parameter 'result' to be an open expression.",
                    nameof(function));
            }
            result = constructionContext.CreateTransformationProvider(projection, context);
        }

        return new Expressif.Library.Array.Generate(
            condition,
            constructionContext.CreateTransformationProvider(next, context),
            result);
    }
}
