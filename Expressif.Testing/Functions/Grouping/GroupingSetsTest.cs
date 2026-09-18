using Expressif.Functions;
using Expressif.Testing.Conformance;
using Expressif.Values;
using GroupingValue = Expressif.Values.Grouping;

namespace Expressif.Testing.Functions.Grouping;

public class GroupingSetsTest
{
    [Conformance]
    public void GroupingSets_Valid_Groups(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [TestCase("#{(T(1, 2) => {3})} | grouping-sets(tuple(-1))")]
    [TestCase("#{(T(1, 2) => {3})} | grouping-sets(tuple(2))")]
    [TestCase("#{(T(1, 2) => {3})} | grouping-sets(tuple(0.5))")]
    [TestCase("#{(T(1, 2) => {3})} | grouping-sets(tuple(\"0\"))")]
    [TestCase("#{(T(1, 2) => {3})} | grouping-sets(tuple(#null))")]
    [TestCase("#{(T(1, 2) => {3})} | grouping-sets(0)")]
    [TestCase("#{} | grouping-sets(tuple(0))")]
    [TestCase("#{(T(1, 2) => {3}), (tuple(1) => {4})} | grouping-sets(tuple())")]
    [TestCase("#{(1 => {3}), (tuple(1) => {4})} | grouping-sets(tuple())")]
    [TestCase("#{(T(1, #all) => {3})} | grouping-sets(tuple())")]
    public void Evaluate_RejectsInvalidSetsAndKeys(string expression)
        => Assert.That(() => Expression.Create(expression).Evaluate(null), Throws.InstanceOf<ArgumentException>());

    [Test]
    public void Bind_RejectsNamedArguments()
        => Assert.That(() => Expression.Create("#{} | grouping-sets(values := tuple())"), Throws.InstanceOf<Expressif.Bindings.BindingException>());

    [TestCase("roll-up", "T(0, 1, 2), T(0, 1), tuple(0), tuple()")]
    [TestCase("cube", "T(0, 1, 2), T(0, 1), T(0, 2), tuple(0), T(1, 2), tuple(1), tuple(2), tuple()")]
    public void Evaluate_MatchesSpecializedExpansion(string specialized, string sets)
    {
        const string source = "#{(T(1, 2, 3) => {4}), (T(1, 5, 3) => {6})}";
        Assert.That(Expression.Create($"{source} | grouping-sets({sets})").Evaluate(null),
            Is.EqualTo(Expression.Create($"{source} | {specialized}").Evaluate(null)));
    }

    [Test]
    public void Evaluate_PreservesReferencesAndResetsBetweenCalls()
    {
        IFunction<GroupingValue, GroupingValue> function = new Expressif.Functions.Grouping.GroupingSets(
            () => [new ValueArgumentEvaluator(_ => new TupleValue(0)), new ValueArgumentEvaluator(_ => new TupleValue())]);
        var item = new object();
        var input = new GroupingValue([new PairValue(new TupleValue(null, 1), new object?[] { item, null, item })]);
        var first = function.Evaluate(input);
        var second = function.Evaluate(input);
        Assert.Multiple(() =>
        {
            Assert.That(((TupleValue)first[0].Key!)[0], Is.Null);
            Assert.That(((TupleValue)first[0].Key!)[1], Is.SameAs(AllDimension.Instance));
            Assert.That(first[1].Values[0], Is.SameAs(item));
            Assert.That(first[1].Values[1], Is.Null);
            Assert.That(first[1].Values[2], Is.SameAs(item));
            Assert.That(second, Is.EqualTo(first));
            Assert.That(((TupleValue)input[0].Key!)[1], Is.EqualTo(1));
        });
    }
}
