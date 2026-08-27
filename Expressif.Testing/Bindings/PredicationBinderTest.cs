using Expressif.Bindings;

namespace Expressif.Testing.Bindings;

public class PredicationBinderTest
{
    private static ExpressifBinder Binder { get; } = new();

    [Test]
    public void Bind_SinglePredication_PreservesFunction()
    {
        var syntax = SyntaxFactory.Open(SyntaxFactory.Function("even"));

        var predication = (SinglePredication)Binder.BindPredication(syntax);

        Assert.That(predication.Member.Name, Is.EqualTo("even"));
    }

    [Test]
    public void Bind_UnaryPredication_LowersToNot()
    {
        var syntax = SyntaxFactory.Open(SyntaxFactory.Unary("!", SyntaxFactory.Function("even")));

        var predication = (PipelinePredication)Binder.BindPredication(syntax);

        Assert.That(predication.Expression.Members.Select(member => member.Name), Is.EqualTo(new[] { "even", "not" }));
    }

    [Test]
    public void Bind_BinaryPredication_LowersToCombinator()
    {
        var syntax = SyntaxFactory.Open(SyntaxFactory.Binary(
            SyntaxFactory.Function("even"),
            "|AND",
            SyntaxFactory.Function("odd")));

        var predication = (BinaryPredication)Binder.BindPredication(syntax);

        Assert.Multiple(() =>
        {
            Assert.That(predication.Operator.Name, Is.EqualTo("and"));
            Assert.That(predication.LeftMember, Is.TypeOf<SinglePredication>());
            Assert.That(predication.RightMember, Is.TypeOf<SinglePredication>());
        });
    }

    [Test]
    public void Bind_ClosedPredication_PreservesSourceMember()
    {
        var syntax = SyntaxFactory.Closed(
            SyntaxFactory.Number(123),
            SyntaxFactory.Unary("!", SyntaxFactory.Function(
                "equal-to",
                SyntaxFactory.Argument(SyntaxFactory.Number(125)))));

        var predication = (PipelinePredication)Binder.BindPredication(syntax);

        Assert.That(predication.Expression.Members.Select(member => member.Name), Is.EqualTo(new[] { "equal-to", "not" }));
    }
}
