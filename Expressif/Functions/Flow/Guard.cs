using Expressif.Functions.Catalog;
using Expressif.Types;

namespace Expressif.Functions.Flow;

/// <summary>
/// Evaluates an expression only when the current input is directly compatible with its entry contract.
/// Otherwise, returns the original input unchanged.
/// </summary>
[Function(prefix: "", aliases: ["guard"])]
[Scope("flow")]
public sealed class Guard : IFunction
{
    private readonly Func<IFunction> expression;

    /// <param name="expression">Expression evaluated when its entry contract directly accepts the current input.</param>
    public Guard(Func<IFunction> expression)
        => this.expression = expression;

    public object? Evaluate(object? value)
    {
        var guarded = expression.Invoke();
        return IsDirectlyCompatible(value, guarded) ? guarded.Evaluate(value) : value;
    }

    private static bool IsDirectlyCompatible(object? value, IFunction function)
    {
        var entry = function is ChainFunction chain
            ? chain.Functions.FirstOrDefault()
            : function;
        if (entry is null)
            return true;

        var documentation = FunctionCatalog.Default.Functions
            .SingleOrDefault(candidate => candidate.Name.Equals(
                entry.GetType().Name.ToKebabCase(), StringComparison.OrdinalIgnoreCase));
        if (documentation is not null && TypeRegistry.TryResolve(documentation.Input, out var expected))
            return TypeRegistry.IsInstance(value, expected);

        var contracts = entry.GetType().GetInterfaces()
            .Where(candidate => candidate.IsGenericType
                && candidate.GetGenericTypeDefinition() == typeof(IFunction<,>))
            .Select(candidate => candidate.GetGenericArguments()[0])
            .Distinct()
            .ToArray();
        return contracts.Length == 0
            || contracts.Any(contract => value is null
                ? !contract.IsValueType || Nullable.GetUnderlyingType(contract) is not null
                : contract.IsInstanceOfType(value));
    }
}
