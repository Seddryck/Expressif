using Expressif.Functions;
using Expressif.Testing.Conformance;
using Expressif.Values;
using Cube = Expressif.Functions.Grouping.Cube;
using GroupingValue = Expressif.Values.Grouping;

namespace Expressif.Testing.Functions.Grouping;

public class CubeTest
{
    [Conformance]
    public void Cube_Valid_Groups(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [TestCase("#{(T(1, 2) => {1}), (tuple(1) => {2})} | cube")]
    [TestCase("#{(1 => {1}), (tuple(1) => {2})} | cube")]
    [TestCase("#{(tuple(1) => {1}), (1 => {2})} | cube")]
    public void Evaluate_RejectsInconsistentDimensions(string expression)
        => Assert.That(() => Expression.Create(expression).Evaluate(null), Throws.ArgumentException.With.Message.StartsWith("Grouping keys must be all scalar"));

    [TestCase("#{(1 => {1})} | cube | cube")]
    [TestCase("#{(T(1, 2) => {1})} | cube | cube")]
    [TestCase("#{(T(1, 2) => {1})} | roll-up | cube")]
    [TestCase("#{(T(1, 2) => {1})} | cube | roll-up")]
    public void Evaluate_RejectsAlreadyExpandedKeys(string expression)
        => Assert.That(() => Expression.Create(expression).Evaluate(null), Throws.ArgumentException.With.Message.StartsWith("Grouping keys must not contain an aggregated dimension"));

    [Test]
    public void Evaluate_TypedContractPreservesValuesAndSharesMarkerWithRollUp()
    {
        IFunction<GroupingValue, GroupingValue> function = new Cube();
        var item = new object();
        var input = new GroupingValue([new PairValue(new Expressif.Values.Tuple(null, 1), new object?[] { item, null, item })]);
        var first = function.Evaluate(input);
        var second = function.Evaluate(input);
        var rollUp = new Expressif.Functions.Grouping.RollUp().Evaluate(input);

        Assert.Multiple(() =>
        {
            Assert.That(first.Count, Is.EqualTo(4));
            Assert.That(((TupleValue)first[2].Key!)[0], Is.SameAs(AllDimension.Instance));
            Assert.That(first[3].Key, Is.EqualTo(rollUp[2].Key));
            Assert.That(first[3].Key!.GetHashCode(), Is.EqualTo(rollUp[2].Key!.GetHashCode()));
            Assert.That(first[3].Values[0], Is.SameAs(item));
            Assert.That(first[3].Values[1], Is.Null);
            Assert.That(first[3].Values[2], Is.SameAs(item));
            Assert.That(second, Is.EqualTo(first));
            Assert.That(input.Count, Is.EqualTo(1));
            Assert.That(((TupleValue)input[0].Key!)[0], Is.Null);
            Assert.That(input[0].Values, Is.EqualTo(new object?[] { item, null, item }));
        });
    }
}
