using Expressif.Values;

namespace Expressif.Functions.Tuple;

/// <summary>
/// Invokes a named callable using the first tuple position as pipeline input and the remaining positions as already-evaluated argument values.
/// </summary>
[Function(prefix: "", DynamicReason = "Output depends on the selected callable signature and tuple item types.")]
[Scope("tuple")]
public sealed class Bind : IFunction<IPositionalValue, object?>
{
    private readonly Func<string> function;
    private readonly Func<string, IPositionalValue, object?> invoke;

    /// <param name="function">Names the callable to invoke.</param>
    public Bind(Func<string> function)
        : this(function, (name, tuple) => new FunctionFactory().InvokeTuple(name, tuple)) { }

    internal Bind(Func<string> function, Func<string, IPositionalValue, object?> invoke)
        => (this.function, this.invoke) = (function, invoke);

    public object? Evaluate(IPositionalValue value) => invoke(function(), value);

    object? IFunction.Evaluate(object? value)
        => value is IPositionalValue tuple ? Evaluate(tuple)
            : throw new Bindings.TupleBindingException(Bindings.TupleBindingFailure.InvalidInput, "bind requires tuple input.");
}
