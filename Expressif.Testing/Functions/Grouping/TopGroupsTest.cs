using Expressif.Functions;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Grouping;

public class TopGroupsTest
{
    [Conformance]
    public void TopGroups_Valid_Groups(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Test]
    public void Evaluate_VisitsGroupsOnceAndPreservesValuesAcrossCalls()
    {
        var observer = new Observer();
        IFunction<Expressif.Values.Grouping, Expressif.Values.Grouping> function =
            new Expressif.Functions.Grouping.TopGroups(() => 2, () => observer);
        var item = new object();
        var input = new Expressif.Values.Grouping([new PairValue(1, new object?[] { item, null, item }), new PairValue(2, new[] { 4 })]);
        var first = function.Evaluate(input);
        var second = function.Evaluate(input);
        Assert.Multiple(() =>
        {
            Assert.That(observer.Keys, Is.EqualTo(new[] { 1, 2, 1, 2 }));
            Assert.That(first[1].Values[0], Is.SameAs(item));
            Assert.That(first[1].Values[1], Is.Null);
            Assert.That(first[1].Values[2], Is.SameAs(item));
            Assert.That(second, Is.EqualTo(first));
            Assert.That(input[0].Key, Is.EqualTo(1));
        });
    }

    [Test]
    public void Evaluate_NormalizesMixedNumericScores()
    {
        var input = new Expressif.Values.Grouping([new PairValue(2, new[] { 1 }), new PairValue(10m, new[] { 2 })]);
        var function = new Expressif.Functions.Grouping.TopGroups(() => 1, () => new Observer());
        Assert.That(function.Evaluate(input)[0].Key, Is.EqualTo(10m));
    }

    [TestCase("#{(1 => {1})} | top-groups(-1, $key)")]
    [TestCase("#{} | top-groups(-1, $key)")]
    [TestCase("#{(1 => {1})} | top-groups(1, $value)")]
    public void Evaluate_RejectsInvalidArguments(string expression)
        => Assert.That(() => Expression.Create(expression).Evaluate(null), Throws.InstanceOf<ArgumentException>());

    private sealed class Observer : IFunction
    {
        public List<object?> Keys { get; } = [];

        public object? Evaluate(object? value)
        {
            var group = (Group)value!;
            Keys.Add(group.Key);
            return group.Key;
        }
    }
}
