using Expressif.Functions;
using Expressif.Discovery;
using Expressif.Library.Special;
using Expressif.Functions.Accumulation;
using Expressif.Predicates;
using Expressif.Values.Types;

namespace Expressif.Bindings;

/// <summary>
/// Composes the Expressif syntax binder with the official built-in vocabulary.
/// </summary>
internal static class ExpressifBinderFactory
{
    private static readonly ITypeSource Source = new AssemblyTypeSource(typeof(ExpressifBinderFactory).Assembly);
    private static readonly FunctionBinderRegistry FunctionBinders = new(
        [
            new CoerceFunctionBinder(),
            new ConditionalFunctionBinder(),
            new ControlFlowFunctionBinder(),
            new FieldFunctionBinder(),
            new LetFunctionBinder(),
            new RecordFunctionBinder(),
            new SortByFunctionBinder(),
            new SortTermFunctionBinder(),
            new ValueSpreadFunctionBinder(),
            new WithFunctionBinder(),
        ]);

    private static readonly ITypeRegistry Types = ExpressifTypeRegistry.Instance;

    internal static ExpressifBinder Create(bool trackSources = false)
        => new(
            [
                new FunctionRegistry(Source),
                new PredicateRegistry(Source),
                new AccumulatorRegistry(Source),
            ],
            FunctionBinders,
            Types,
            trackSources);
}
