using Expressif.Functions;
using Expressif.Testing.Conformance;
using Expressif.Values;
using GroupingValue = Expressif.Values.Grouping;
using RollUp = Expressif.Functions.Grouping.RollUp;

namespace Expressif.Testing.Functions.Grouping;

public class RollUpTest
{
    [Conformance]
    public void RollUp_Valid_Groups(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [TestCase("#{(T(1, 2) => {1}), (tuple(1) => {2})} | roll-up")]
    [TestCase("#{(1 => {1}), (tuple(1) => {2})} | roll-up")]
    [TestCase("#{(tuple(1) => {1}), (1 => {2})} | roll-up")]
    public void Evaluate_RejectsInconsistentDimensions(string expression)
        => Assert.That(() => Expression.Create(expression).Evaluate(null), Throws.ArgumentException.With.Message.StartsWith("Grouping keys must be all scalar"));

    [TestCase("#{(1 => {1})} | roll-up | roll-up")]
    [TestCase("#{(tuple(1) => {1})} | roll-up | roll-up")]
    public void Evaluate_RejectsAlreadyExpandedKeys(string expression)
        => Assert.That(() => Expression.Create(expression).Evaluate(null), Throws.ArgumentException.With.Message.StartsWith("Grouping keys must not contain an aggregated dimension"));

    [Test]
    public void Evaluate_TypedContractPreservesValuesAndIsReusable()
    {
        IFunction<GroupingValue, GroupingValue> function = new RollUp();
        var item = new object();
        var input = new GroupingValue([new PairValue(null, new object?[] { item, null, item })]);
        var first = function.Evaluate(input);
        var second = function.Evaluate(input);

        Assert.Multiple(() =>
        {
            Assert.That(first[0].Key, Is.Null);
            Assert.That(first[1].Key, Is.SameAs(AllDimension.Instance));
            Assert.That(first[1].Values[0], Is.SameAs(item));
            Assert.That(first[1].Values[1], Is.Null);
            Assert.That(first[1].Values[2], Is.SameAs(item));
            Assert.That(second, Is.EqualTo(first));
            Assert.That(input.Count, Is.EqualTo(1));
            Assert.That(input[0].Key, Is.Null);
            Assert.That(input[0].Values, Is.EqualTo(new object?[] { item, null, item }));
            Assert.That(object.Equals(AllDimension.Instance, null), Is.False);
            Assert.That(object.Equals(AllDimension.Instance, "#all"), Is.False);
        });
    }
}
