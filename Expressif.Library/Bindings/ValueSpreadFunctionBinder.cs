using Expressif.Functions;
using Expressif.Functions.Grouping;
using Expressif.Functions.Record;
using Expressif.Functions.Text;
using Expressif.Syntax;
using ArrayFunction = Expressif.Functions.Array.Array;
using DictionaryFunction = Expressif.Functions.Dictionary.Dictionary;
using GroupingFunction = Expressif.Functions.Grouping.Grouping;
using TextFunction = Expressif.Functions.Text.Text;
using TupleFunction = Expressif.Functions.Tuple.Tuple;

namespace Expressif.Bindings;

internal sealed class ValueSpreadFunctionBinder :
    IFunctionBinder<ArrayFunction>,
    IFunctionBinder<TextFunction>,
    IFunctionBinder<TupleFunction>,
    IFunctionBinder<GroupingFunction>,
    IFunctionBinder<GroupingSets>,
    IFunctionBinder<DictionaryFunction>,
    IFunctionBinder<NestedField>,
    IFunctionBinder<SplitLengths>
{
    public Function Bind(FunctionCallSyntax syntax, IFunctionBindingContext context)
    {
        var arguments = new List<FunctionArgument>();
        foreach (var argument in syntax.Arguments)
        {
            if (argument is NamedArgumentSyntax)
                throw new BindingException($"Function '{syntax.Name}' does not support named arguments.");
            arguments.Add(new FunctionArgument(
                null,
                argument is SpreadArgumentSyntax { IsImplicitSpread: true }
                    ? new IncomingValueParameter()
                    : context.BindArgument(argument.Value
                        ?? throw new BindingException("An explicit spread argument must include an expression.")),
                argument is SpreadArgumentSyntax));
        }
        return Function.FromArguments(syntax.Name, arguments.ToArray());
    }
}
