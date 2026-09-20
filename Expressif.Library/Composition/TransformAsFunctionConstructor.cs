using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal sealed class TransformAsFunctionConstructor : IFunctionConstructor<Expressif.Library.Flow.TransformAs>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
    {
        if (function.Arguments.Length < 2
            || function.Arguments[0].Name is not null
            || !FunctionConstructorSupport.TryGetOpenExpression(function.Arguments[0].Value, out var open)
            || function.Arguments.Skip(1).Any(argument => argument.Name is null || argument.IsSpread))
        {
            throw new BindingException(
                $"The function named '{function.Name}' expects one positional open expression followed by one or more named expressions.");
        }

        var names = new HashSet<string>(StringComparer.Ordinal);
        var duplicate = function.Arguments.Skip(1)
            .FirstOrDefault(argument => !names.Add(argument.Name!));
        if (duplicate is not null)
            throw new BindingException($"Duplicate named expression '{duplicate.Name}' in {function.Name}(...).");

        var operation = constructionContext.CreateOpenExpression(open.Expression, context);
        var expressions = function.Arguments.Skip(1)
            .Select(argument => new Expressif.Library.Flow.NamedExpressionEvaluator(
                argument.Name!,
                constructionContext.CreateValueEvaluator(argument.Value, context)));
        return new Expressif.Library.Flow.TransformAs(
            () => new LexicallyBoundContextFunction(operation),
            expressions);
    }
}
