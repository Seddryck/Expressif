using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal sealed class TransformAsFunctionConstructor : IFunctionConstructor<Expressif.Library.Flow.TransformAs>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
    {
        var layout = ParameterArgumentBinder.BindLayout(typeof(Expressif.Library.Flow.TransformAs), function.Arguments);
        if (!FunctionConstructorSupport.TryGetOpenExpression(layout.Positional[0].Value, out var open))
        {
            throw new BindingException(
                $"The function named '{function.Name}' expects one positional open expression followed by one or more named expressions.");
        }

        var operation = constructionContext.CreateOpenExpression(open.Expression, context);
        var expressions = layout.Named
            .Select(argument => new Expressif.Library.Flow.NamedExpressionEvaluator(
                argument.Name!,
                constructionContext.CreateValueEvaluator(argument.Value, context)));
        return new Expressif.Library.Flow.TransformAs(
            () => new LexicallyBoundContextFunction(operation),
            expressions);
    }
}
