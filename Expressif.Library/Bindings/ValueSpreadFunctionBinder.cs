using Expressif.Functions;
using Expressif.Library.Flow;
using Expressif.Library.Grouping;
using Expressif.Library.Numeric;
using Expressif.Library.Record;
using Expressif.Library.Temporal;
using Expressif.Library.Text;
using Expressif.Syntax;
using ArrayFunction = Expressif.Library.Array.Array;
using DictionaryFunction = Expressif.Library.Dictionary.Dictionary;
using GroupingFunction = Expressif.Library.Grouping.Grouping;
using TextFunction = Expressif.Library.Text.Concatenation.Text;
using TupleFunction = Expressif.Library.Tuple.Tuple;
using SplitLengths = Expressif.Library.Text.Partitioning.SplitLengths;

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
