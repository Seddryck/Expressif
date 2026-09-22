using Expressif.Functions;
using Expressif.Functions.Coercions;
using Expressif.Discovery;
using Expressif.Library.Special;
using Expressif.Functions.Accumulation;
using Expressif.Predicates;
using Expressif.Values.Types;

namespace Expressif.Bindings;

/// <summary>
/// Composes the Expressif syntax binder with the official built-in vocabulary.
/// </summary>
public static class ExpressifBinderFactory
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

    private static readonly ICoercionRegistry Coercions = new CoercionRegistry(Source);
    private static readonly ITypeRegistry Types = ExpressifTypeRegistry.Instance;

    public static ExpressifBinder Create(bool applyCoercion = true)
        => Create(applyCoercion, false);

    internal static ExpressifBinder Create(bool applyCoercion, bool trackSources)
        => new(
            [
                new FunctionRegistry(Source),
                new PredicateRegistry(Source),
                new AccumulatorRegistry(Source),
            ],
            FunctionBinders,
            Types,
            Coercions,
            applyCoercion,
            trackSources);
}
