using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Grouping;

public class DrillUpTest
{
    [Test]
    public void Evaluate_VisitsOnlyKeysIncludingEmptyGroupsAndResetsBetweenCalls()
    {
        var observer = new KeyObserver();
        var function = new Expressif.Functions.Grouping.DrillUp(observer.Evaluate);
        var input = new Expressif.Values.Grouping([
            new PairValue("B", new[] { 2, 1 }),
            new PairValue("A", System.Array.Empty<object?>()),
            new PairValue("C", new[] { 3 }),
        ]);

        var first = function.Evaluate(input);
        var second = function.Evaluate(input);

        Assert.Multiple(() =>
        {
            Assert.That(observer.Keys, Is.EqualTo(new[] { "B", "A", "C", "B", "A", "C" }));
            Assert.That(first[0].Values, Is.EqualTo(new[] { 2, 1, 3 }));
            Assert.That(second, Is.EqualTo(first));
            Assert.That(input[0].Values, Is.EqualTo(new[] { 2, 1 }));
        });
    }

    private sealed class KeyObserver : Expressif.Functions.IFunction
    {
        public List<object?> Keys { get; } = [];

        public object? Evaluate(object? value)
        {
            Keys.Add(value);
            return null;
        }
    }

    [Conformance]
    public void DrillUp_Valid_Groups(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));
}
