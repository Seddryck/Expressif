using Expressif.Introspection;
using Expressif.Functions;
using Expressif.Library.Text;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Text;

[TestFixture]
public class CurrencyTest
{
    [Conformance]
    public void IsCurrencySymbol_Valid(string? value, bool expected)
    {
        IFunction<string?, bool> predicate = new IsCurrencySymbol();
        Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
        Assert.That(((IFunction)predicate).Evaluate(value), Is.EqualTo(expected));
    }

    [Conformance]
    public void MatchesCurrency_Valid(string? value, bool expected)
    {
        IFunction<string?, bool> predicate = new MatchesCurrency();
        Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
        Assert.That(((IFunction)predicate).Evaluate(value), Is.EqualTo(expected));
    }

    [TestCase("\"$\" | is-currency-symbol", true)]
    [TestCase("\"$1,000.01\" | matches-currency", true)]
    [TestCase("\"$5,00\" | matches-currency", false)]
    [TestCase("{\"$1\", \"$5,00\", \"1€\"} | filter(matches-currency) | count", 2)]
    [TestCase("{\"$\", \"USD\", \"€\"} | filter(is-currency-symbol) | count", 2)]
    public void Currency_ComposesWithPipeline(string expression, object expected)
        => Assert.That(new ExpressionFactory(new ExpressionBinder()).Create(expression).Evaluate(null), Is.EqualTo(expected));

    [TestCase("is-currency-symbol", typeof(IsCurrencySymbol))]
    [TestCase("matches-currency", typeof(MatchesCurrency))]
    public void Currency_RegisteredWithTypedContract(string name, Type implementation)
    {
        var info = ExpressifIntrospection.Predicates.Describe().Single(x => x.Name == name);
        Assert.That(info.ImplementationType, Is.EqualTo(implementation));
        Assert.That(info.Scope, Is.EqualTo("text"));
        Assert.That(info.Aliases, Is.Empty);
        Assert.That(info.Parameters, Is.Empty);
        Assert.That(typeof(IFunction<string?, bool>).IsAssignableFrom(implementation), Is.True);
    }

    [TestCase("\ud800")]
    [TestCase("\udc00")]
    [TestCase("$1\ud800")]
    [TestCase("1\udc00$")]
    [TestCase("$1\n0")]
    public void Currency_RejectsMalformedText(string value)
    {
        Assert.That(new IsCurrencySymbol().Evaluate(value), Is.False);
        Assert.That(new MatchesCurrency().Evaluate(value), Is.False);
    }
}
