using Expressif.Functions;
using Expressif.Functions.Coercions;
using Expressif.Introspection;

namespace Expressif.Testing.Functions.Coercions;

public sealed class CoercionRegistryContractTest
{
    [Test]
    public void DescribeUsesDescriptorSourcesAndConstructedFunctionType()
    {
        var introspector = new CoercionIntrospector([
            new SampleDescriptor("convert", typeof(decimal), new HashSet<Type> { typeof(int), typeof(string) }),
        ]);

        var descriptions = introspector.Describe().ToArray();

        Assert.That(descriptions.Select(description => (
            description.Name,
            description.SourceType,
            description.TargetType,
            description.ImplementationType)), Is.EquivalentTo(new[]
        {
            ("convert", typeof(int), typeof(decimal), typeof(SampleFunction)),
            ("convert", typeof(string), typeof(decimal), typeof(SampleFunction)),
        }));
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

        public Type GetImplementationType(Type sourceType) => typeof(SampleFunction);

        public IFunction Create(Type sourceType)
            => new SampleFunction();
    }

    private sealed class SampleFunction : IFunction
    {
        public object? Evaluate(object? value) => value;
    }
}
