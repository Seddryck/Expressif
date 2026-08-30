using Expressif.Functions.Introspection;
using Expressif.Testing.Conformance;
using Expressif.Types;
using TypesFunction = Expressif.Functions.Introspection.Types;

namespace Expressif.Testing.Functions.Introspection;

public class TypesTest
{
    [Conformance]
    public void Types_Valid_CanonicalNames(object? value, string[] expected)
        => Assert.That(new TypesFunction().Evaluate(value).Select(type => type.Name), Is.EqualTo(expected));

    [Test]
    public void Expression_Types_ReturnsCanonicalRegistry()
        => Assert.That(
            ((TypeDescriptor[])Expression.Create("types()").Evaluate(null)!).Select(type => type.Name),
            Is.EqualTo(TypeRegistry.All.Select(type => type.Name).Order()));
}
