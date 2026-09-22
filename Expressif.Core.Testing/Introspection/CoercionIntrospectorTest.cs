using System.Reflection;
using Expressif.Discovery;
using Expressif.Functions;
using Expressif.Functions.Coercions;
using Expressif.Introspection;

namespace Expressif.Testing.Introspection;

public class CoercionIntrospectorTest
{
    [Test]
    public void RegistryContract_OnlyExposesRuntimeResolutionAndCreation()
    {
        Assert.Multiple(() =>
        {
            Assert.That(typeof(ICoercionRegistry).GetProperties(), Is.Empty);
            Assert.That(
                typeof(ICoercionRegistry).GetMethods().Select(method => method.Name).Distinct(),
                Is.EquivalentTo(new[] { "TryResolve", "TryCreate" }));
        });
    }

    [Test]
    public void Describe_UsesSourceSpecificMetadataWithoutCreatingFunctions()
    {
        SideEffectDescriptor.CreateCalls = 0;
        var introspector = new CoercionIntrospector(new FixedTypeSource(typeof(SideEffectDescriptor)));

        var first = introspector.Describe();
        var second = introspector.Describe();

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.SameAs(second));
            Assert.That(SideEffectDescriptor.CreateCalls, Is.Zero);
            Assert.That(
                first.ToDictionary(info => info.SourceType, info => info.ImplementationType),
                Is.EquivalentTo(new Dictionary<Type, Type>
                {
                    [typeof(int)] = typeof(IntegerFunction),
                    [typeof(string)] = typeof(TextFunction),
                }));
            Assert.That(() => ((IList<CoercionInfo>)first).Add(first[0]), Throws.TypeOf<NotSupportedException>());
        });
    }

    [Test]
    public void PublicSurface_IsSealedAndOnlyDescribes()
    {
        Assert.Multiple(() =>
        {
            Assert.That(typeof(CoercionIntrospector).IsSealed, Is.True);
            Assert.That(
                typeof(CoercionIntrospector)
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Select(method => method.Name),
                Is.EqualTo(new[] { nameof(CoercionIntrospector.Describe) }));
        });
    }

    private sealed class FixedTypeSource(params Type[] types) : ITypeSource
    {
        public IEnumerable<Type> GetTypes() => types;
    }

    private sealed class SideEffectDescriptor : ICoercionDescriptor
    {
        public static int CreateCalls { get; set; }
        public string Name => "side-effect";
        public Type TargetType => typeof(object);
        public IReadOnlySet<Type> SourceTypes { get; } = new HashSet<Type> { typeof(int), typeof(string) };

        public bool Supports(Type sourceType, Type targetType)
            => SourceTypes.Contains(sourceType) && targetType == TargetType;

        public Type GetImplementationType(Type sourceType)
            => sourceType == typeof(int) ? typeof(IntegerFunction) : typeof(TextFunction);

        public IFunction Create(Type sourceType)
        {
            CreateCalls++;
            throw new InvalidOperationException("Introspection must not create runtime functions.");
        }
    }

    private sealed class IntegerFunction : IFunction
    {
        public object? Evaluate(object? value) => value;
    }

    private sealed class TextFunction : IFunction
    {
        public object? Evaluate(object? value) => value;
    }
}
