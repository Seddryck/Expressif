using Expressif.Functions.Coercions;
using Expressif.Values.Types;

namespace Expressif.Introspection;

internal readonly record struct ParameterIntrospectionKey(Type DeclaringType, string ParameterName);

/// <summary>
/// Supplies vocabulary-specific metadata to the Core introspection engine.
/// </summary>
internal sealed record IntrospectionOptions(
    ITypeRegistry Types,
    ICoercionRegistry Coercions,
    IReadOnlyDictionary<ParameterIntrospectionKey, string> ParameterTypes,
    IReadOnlyDictionary<ParameterIntrospectionKey, string> ParameterNames,
    IReadOnlyDictionary<ParameterIntrospectionKey, int> VariadicParameters,
    IReadOnlyDictionary<string, string> UntypedReasons,
    IReadOnlyDictionary<string, string> OutputOverrides,
    Func<Type, IReadOnlyList<TupleBindingSignature>> TupleBindingSignatures)
{
    internal static IntrospectionOptions Default { get; } = new(
        new TypeRegistry(Array.Empty<TypeDescriptor>()),
        new CoercionRegistry(Array.Empty<ICoercionDescriptor>()));

    public IntrospectionOptions(ITypeRegistry types, ICoercionRegistry coercions)
        : this(
            types,
            coercions,
            new Dictionary<ParameterIntrospectionKey, string>(),
            new Dictionary<ParameterIntrospectionKey, string>(),
            new Dictionary<ParameterIntrospectionKey, int>(),
            new Dictionary<string, string>(StringComparer.Ordinal),
            new Dictionary<string, string>(StringComparer.Ordinal),
            _ => []) { }
}
