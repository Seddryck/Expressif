using Expressif.Testing.Conformance;
using Expressif.Values;
using GroupingValue = Expressif.Values.Grouping;

namespace Expressif.Testing.Functions.Grouping;

public class GroupingTest
{
    [Conformance]
    public void Grouping_Valid_Constructor(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Conformance]
    public void Grouping_Valid_Literal(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Conformance]
    public void Grouping_Valid_Accessors(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Conformance]
    public void Summarize_Valid_Groups(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Conformance]
    public void Summarize_Valid_Empty(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Conformance]
    public void MapGroups_Valid_Groups(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Conformance]
    public void MapGroups_Valid_Empty(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Conformance]
    public void FilterGroups_Valid_Groups(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Conformance]
    public void FilterGroups_Valid_Empty(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Test]
    public void Constructor_ConvertsPairsToGroupsAndPreservesOrder()
    {
        var grouping = new GroupingValue([
            new PairValue("BE", new object?[] { "alice", "bob" }),
            new PairValue("FR", new object?[] { "charlie" }),
        ]);

        Assert.Multiple(() =>
        {
            Assert.That(grouping.Select(group => group.Key), Is.EqualTo(new[] { "BE", "FR" }));
            Assert.That(grouping, Has.All.TypeOf<Group>());
        });
    }

    [Test]
    public void Constructor_DuplicateStructuralKeys_ThrowsExplicitly()
        => Assert.That(
            () => new GroupingValue([
                new PairValue(new object?[] { 1m, 2m }, new object?[] { "first" }),
                new PairValue(new object?[] { 1m, 2m }, new object?[] { "second" }),
            ]),
            Throws.ArgumentException.With.Message.StartsWith("A grouping cannot contain duplicate key"));

    [TestCase("grouping(42)", "Every grouping argument must evaluate to a pair.")]
    [TestCase("grouping((\"BE\" => 42))", "Every grouping entry value must be a collection.")]
    [TestCase("grouping((\"BE\" => {1}), (\"BE\" => {2}))", "A grouping cannot contain duplicate key")]
    public void Constructor_InvalidEntry_ThrowsExplicitly(string expression, string message)
        => Assert.That(
            () => Expression.Create(expression).Evaluate(null),
            Throws.ArgumentException.With.Message.StartsWith(message));

    [Test]
    public void Literal_RoundTripsGroupingAndGroupRuntimeTypes()
    {
        var source = new GroupingValue([
            new PairValue("BE", new object?[] { "alice", "bob" }),
            new PairValue("FR", new object?[] { "charlie" }),
        ]);

        var parsed = Expression.CreateClosed(ValueFormatter.Format(source)).Evaluate(null);

        Assert.Multiple(() =>
        {
            Assert.That(parsed, Is.TypeOf<GroupingValue>().And.EqualTo(source));
            Assert.That(((GroupingValue)parsed!).ToArray(), Has.All.TypeOf<Group>());
        });
    }

    [Test]
    public void OrdinaryPairLiteral_RemainsPair()
        => Assert.That(
            Expression.CreateClosed("(\"BE\" => {\"alice\"})").Evaluate(null),
            Is.TypeOf<Expressif.Values.Pair>());

    [Test]
    public void Group_IsTransparentToArrayFunctions()
        => Assert.That(
            Expression.CreateClosed("#{(\"BE\" => {\"alice\", \"bob\"}), (\"FR\" => {\"charlie\"})} | map(cardinality)").Evaluate(null),
            Is.EqualTo(new object?[] { 2, 1 }));
}
