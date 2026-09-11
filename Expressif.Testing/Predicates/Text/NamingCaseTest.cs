using Expressif.Functions;
using Expressif.Predicates.Text;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Predicates.Text;

[TestFixture]
public class NamingCaseTest
{
    [Conformance]
    public void IsKebabCase_Valid(string? value, bool expected)
    {
        IFunction<string?, bool> predicate = new IsKebabCase();
        Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
        Assert.That(((IFunction)predicate).Evaluate(value), Is.EqualTo(expected));
    }

    [Conformance]
    public void IsSnakeCase_Valid(string? value, bool expected)
    {
        IFunction<string?, bool> predicate = new IsSnakeCase();
        Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
        Assert.That(((IFunction)predicate).Evaluate(value), Is.EqualTo(expected));
    }

    [Conformance]
    public void IsCamelCase_Valid(string? value, bool expected)
    {
        IFunction<string?, bool> predicate = new IsCamelCase();
        Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
        Assert.That(((IFunction)predicate).Evaluate(value), Is.EqualTo(expected));
    }

    [Conformance]
    public void IsPascalCase_Valid(string? value, bool expected)
    {
        IFunction<string?, bool> predicate = new IsPascalCase();
        Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
        Assert.That(((IFunction)predicate).Evaluate(value), Is.EqualTo(expected));
    }
    [TestCase("\"first-name\" | is-kebab-case", true)]
    [TestCase("\"first_name\" | is-snake-case", true)]
    [TestCase("\"httpServerURL\" | is-camel-case", true)]
    [TestCase("\"HTTPServer\" | is-pascal-case", true)]
    [TestCase("\"first--name\" | is-kebab-case", false)]
    [TestCase("\"first--name\" | tokenize-kebab | kebab-case | is-kebab-case", true)]
    [TestCase("\"first__name\" | tokenize-snake | snake-case | is-snake-case", true)]
    [TestCase("\"version2HTTPServer\" | tokenize-camel | camel-case | is-camel-case", true)]
    [TestCase("\"HTTPServer\" | tokenize-pascal | pascal-case | is-pascal-case", true)]
    [TestCase("{\"first-name\", \"first--name\"} | filter(is-kebab-case) | count", 1)]
    public void NamingCase_ComposesWithPipeline(string expression, object expected)
        => Assert.That(new ExpressionFactory().Create(expression).Evaluate(null), Is.EqualTo(expected));

    [TestCase("is-kebab-case", typeof(IsKebabCase))]
    [TestCase("is-snake-case", typeof(IsSnakeCase))]
    [TestCase("is-camel-case", typeof(IsCamelCase))]
    [TestCase("is-pascal-case", typeof(IsPascalCase))]
    public void NamingCase_RegisteredWithTypedContract(string name, Type implementation)
    {
        var info = new Expressif.Predicates.Introspection.PredicateIntrospector()
            .Describe().Single(x => x.Name == name);
        Assert.That(info.ImplementationType, Is.EqualTo(implementation));
        Assert.That(info.Scope, Is.EqualTo("text"));
        Assert.That(info.Aliases, Is.Empty);
        Assert.That(info.Parameters, Is.Empty);
        Assert.That(typeof(IFunction<string?, bool>).IsAssignableFrom(implementation), Is.True);
    }
}
