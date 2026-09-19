using Expressif.Functions;
using Expressif.Functions.Flow;
using Expressif.Syntax;

namespace Expressif.Bindings;

internal sealed class LetFunctionBinder : IFunctionBinder<Let>
{
    public Function Bind(FunctionCallSyntax syntax, IFunctionBindingContext context)
    {
        if (syntax.Arguments.Count == 0 || syntax.Arguments.Any(argument => argument is not NamedArgumentSyntax))
            throw new BindingException("Function 'let' expects one or more named assignments.");
        var names = new HashSet<string>(StringComparer.Ordinal);
        var bindings = syntax.Arguments.Cast<NamedArgumentSyntax>().Select(named =>
        {
            if (named.Name.QuotingStyle is not null || named.Name.IsPrivate)
                throw new BindingException("Let binding names must be unquoted value identifiers.");
            try
            {
                ExpressifSyntax.Parse($"@_ | {named.Name.Value} :> identity");
            }
            catch (ExpressifSyntaxException exception)
            {
                throw new BindingException($"Invalid let binding name '{named.Name.Value}': {exception.Message}");
            }
            if (!names.Add(named.Name.Value))
                throw new BindingException($"Duplicate let binding name '{named.Name.Value}'.");
            return new LetBinding(named.Name.Value, context.BindArgument(named.Value));
        }).ToArray();
        return new Function(syntax.Name, [new LetDefinitionParameter(bindings)]);
    }
}
