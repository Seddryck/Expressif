using Expressif.Functions;
using Expressif.Functions.Coercions;
using System.Diagnostics.CodeAnalysis;

namespace Expressif.Testing.Functions.Coercions;

public sealed class CoercionRegistryContractTest
{
    [Test]
    public void DescribeUsesDescriptorSourcesAndConstructedFunctionType()
    {
        ICoercionRegistry registry = new MinimalRegistry([
            new SampleDescriptor("convert", typeof(decimal), new HashSet<Type> { typeof(int), typeof(string) }),
        ]);

        var descriptions = registry.Describe().ToArray();

        Assert.That(descriptions, Is.EquivalentTo(new[]
        {
            new CoercionInfo("convert", typeof(int), typeof(decimal), typeof(SampleFunction)),
            new CoercionInfo("convert", typeof(string), typeof(decimal), typeof(SampleFunction)),
        }));
    }

    private sealed class MinimalRegistry(IReadOnlyList<ICoercionDescriptor> descriptors)
        : ICoercionRegistry
    {
        public IReadOnlyList<ICoercionDescriptor> Descriptors { get; } = descriptors;

        public bool TryResolve(
            Type sourceType,
            Type targetType,
            [NotNullWhen(true)] out string? functionName)
        {
            functionName = null;
            return false;
        }
    }

    private sealed class SampleDescriptor(
        string name,
        Type targetType,
        IReadOnlySet<Type> sourceTypes) : ICoercionDescriptor
    {
        public string Name { get; } = name;
        public Type TargetType { get; } = targetType;
        public IReadOnlySet<Type> SourceTypes { get; } = sourceTypes;

        public bool Supports(Type sourceType, Type targetType)
            => SourceTypes.Contains(sourceType) && TargetType == targetType;

        public IFunction Create(Type sourceType)
            => new SampleFunction();
    }

    private sealed class SampleFunction : IFunction
    {
        public object? Evaluate(object? value) => value;
    }
}
