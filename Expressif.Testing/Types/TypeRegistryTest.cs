using System.Text.Json;
using Expressif.Bindings;
using Expressif.Functions.Special;
using Expressif.Functions.Introspection;
using Expressif.Syntax;
using Expressif.Types;
using Expressif.Values;

namespace Expressif.Testing.Types;

[TestFixture]
[Category("MetadataConsistency")]
public class TypeRegistryTest
{
    private static readonly string[] RequiredNames =
        ["boolean", "integer", "text", "numeric", "date", "datetime", "time"];

    [Test]
    public void GeneratedCatalog_MatchesCanonicalRegistry()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Documentation", "type.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var generated = document.RootElement.EnumerateArray()
            .Select(element => element.GetProperty("Name").GetString())
            .ToArray();

        Assert.That(generated, Is.EqualTo(TypeRegistry.All.Select(type => type.Name).Order()));
    }

    [Test]
    public void Registry_ContainsRequiredValueTypesAndExcludesMetadataConcepts()
    {
        var names = TypeRegistry.All.Select(type => type.Name).ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(names, Does.Contain(RequiredNames[0]).And.Contain(RequiredNames[1])
                .And.Contain(RequiredNames[2]).And.Contain(RequiredNames[3])
                .And.Contain(RequiredNames[4]).And.Contain(RequiredNames[5]).And.Contain(RequiredNames[6]));
            Assert.That(names, Does.Not.Contain("expression").And.Not.Contain("predicate").And.Not.Contain("accumulator"));
        });
    }

    [TestCase("integer", "numeric")]
    [TestCase("decimal", "numeric")]
    [TestCase("date", "temporal")]
    [TestCase("datetime", "temporal")]
    [TestCase("time", "temporal")]
    [TestCase("pair", "tuple")]
    public void Registry_ParentMatchesCanonicalHierarchy(string name, string parent)
        => Assert.That(TypeRegistry.Resolve(name).Parent, Is.EqualTo(parent));

    [TestCase("integer", typeof(int))]
    [TestCase("numeric", typeof(decimal))]
    [TestCase("boolean", typeof(bool))]
    [TestCase("text", typeof(string))]
    [TestCase("date", typeof(DateOnly))]
    [TestCase("datetime", typeof(DateTime))]
    [TestCase("time", typeof(TimeOnly))]
    public void Registry_DotNetBindingMatchesCanonicalRuntimeType(string name, Type expected)
        => Assert.That(TypeRegistry.Resolve(name).Bindings["dotnet"], Is.EqualTo(expected.FullName));

    [TestCase("integer", typeof(int))]
    [TestCase("numeric", typeof(decimal))]
    [TestCase("boolean", typeof(bool))]
    [TestCase("text", typeof(string))]
    [TestCase("date", typeof(DateOnly))]
    [TestCase("datetime", typeof(DateTime))]
    [TestCase("date-time", typeof(DateTime))]
    [TestCase("tuple", typeof(TupleValue))]
    [TestCase("record", typeof(RecordValue))]
    public void RuntimeRegistry_ResolvesImplementationTypeDirectly(string name, Type expected)
        => Assert.That(RuntimeTypeRegistry.Resolve(name), Is.EqualTo(expected));

    [Test]
    public void RuntimeRegistry_FamilyWithoutRuntimeType_ReturnsNull()
        => Assert.That(RuntimeTypeRegistry.Resolve("scalar"), Is.Null);

    [Test]
    public void LiteralExamples_AreAcceptedByParser()
    {
        var examples = TypeRegistry.All
            .Where(type => type.Literal is not null)
            .SelectMany(type => type.Literal!.Examples.Select(example => (type.Name, Example: example)));

        Assert.Multiple(() =>
        {
            foreach (var example in examples)
                Assert.That(() => ExpressionParser.Parse(example.Example), Throws.Nothing, $"{example.Name}: {example.Example}");
        });
    }

    [TestCase("integer")]
    [TestCase("boolean")]
    [TestCase("text")]
    public void Binder_TypeLiteral_ResolvesCanonicalDescriptor(string name)
    {
        var parameter = new ExpressifBinder().BindParameter(ExpressionParser.Parse($":{name}"));
        Assert.That(parameter, Is.EqualTo(new LiteralParameter(TypeRegistry.Resolve(name))));
    }

    [Test]
    public void Binder_UnknownTypeLiteral_ThrowsSpecificError()
        => Assert.That(
            () => new ExpressifBinder().BindParameter(ExpressionParser.Parse(":expression")),
            Throws.TypeOf<UnknownExpressifTypeException>());

    [Test]
    public void CoercionBindings_MatchActualReturnTypes()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new CoerceInt().Evaluate("42"), Is.TypeOf<int>());
            Assert.That(new CoerceNumeric().Evaluate("42.5"), Is.TypeOf<decimal>());
        });
    }

    [TestCase("year-month", typeof(YearMonth))]
    [TestCase("weekday", typeof(Weekday))]
    [TestCase("tuple", typeof(TupleValue))]
    [TestCase("record", typeof(RecordValue))]
    public void ValueTypeSummary_ComesFromImplementationDocumentation(string name, Type implementationType)
        => Assert.That(TypeRegistry.Resolve(name).Summary, Is.EqualTo(implementationType.GetSummary()));

    [Test]
    public void Introspector_UnionsIntrinsicDescriptorsAndImplementedValueTypes()
    {
        var names = new TypeIntrospector().Describe().Select(descriptor => descriptor.Name).ToArray();
        Assert.That(names, Does.Contain("numeric").And.Contain("tuple").And.Contain("record"));
    }

    [Test]
    public void IntrinsicDescriptors_OnlyBindSystemTypes()
    {
        var descriptorTypes = typeof(TypeRegistry).Assembly.GetTypes()
            .Where(type => !type.IsAbstract && typeof(ITypeDescriptor).IsAssignableFrom(type))
            .Select(type => (ITypeDescriptor)Activator.CreateInstance(type)!)
            .ToArray();

        Assert.That(
            descriptorTypes.Where(descriptor => descriptor.RuntimeType is not null)
                .Select(descriptor => descriptor.RuntimeType!.Namespace),
            Is.All.EqualTo("System"));
    }
}
