using Expressif.Functions;
using Expressif.Syntax;
using RecordFunction = Expressif.Functions.Record.Record;

namespace Expressif.Bindings;

internal sealed class RecordFunctionBinder : IFunctionBinder<RecordFunction>
{
    public Function Bind(FunctionCallSyntax syntax, IFunctionBindingContext context)
        => syntax.Arguments.Count == 0
            ? new Function(syntax.Name, [])
            : new Function(syntax.Name, [new RecordDefinitionParameter(syntax.Arguments.Select(argument => BindEntry(argument, context)).ToArray())]);

    private static IRecordDefinitionEntry BindEntry(ArgumentSyntax syntax, IFunctionBindingContext context)
        => syntax switch
        {
            NamedArgumentSyntax named => new RecordNamedEntry(named.Name.Value, context.BindArgument(named.Value)),
            SpreadArgumentSyntax spread => new RecordSpreadEntry(
                spread.IsImplicitSpread
                    ? new IncomingValueParameter()
                    : context.BindArgument(spread.Value
                        ?? throw new BindingException("An explicit record spread must include an expression."))),
            PositionalArgumentSyntax { Value: IncomingValueSyntax } => new RecordSpreadEntry(new IncomingValueParameter()),
            _ => throw FunctionBinderSyntax.Unsupported(syntax),
        };
}
