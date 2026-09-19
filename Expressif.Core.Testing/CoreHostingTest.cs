using Expressif.Bindings;
using Expressif.Functions;
using Expressif.Syntax;
using NUnit.Framework;

namespace Expressif.Core.Testing;

public sealed class CoreHostingTest
{
    [Test]
    public void CoreOnlyHostCanParseBindAndEvaluateCustomVocabulary()
    {
        var syntax = ExpressionParser.Parse("double");
        var expression = new CustomBinder().Bind(syntax);

        Assert.That(expression.Evaluate(21m), Is.EqualTo(42m));
    }

    [Test]
    public void CoreOnlyHostDoesNotCopyOfficialLibraryAssembly()
        => Assert.That(
            File.Exists(Path.Combine(AppContext.BaseDirectory, "Expressif.Library.dll")),
            Is.False);

    private sealed class CustomBinder : IExpressionBinder
    {
        public IExpression Bind(RootExpressionSyntax syntax)
        {
            Assert.That(syntax.Text, Is.EqualTo("double"));
            return new CustomExpression(new DoubleFunction());
        }

        public IExpression BindClosed(RootExpressionSyntax syntax)
            => Bind(syntax);
    }

    private sealed class CustomExpression(IFunction function) : IExpression
    {
        public object? Evaluate(object? value) => function.Evaluate(value);

        public IExpression WithContext(EvaluationContext context) => this;
    }

    private sealed class DoubleFunction : Function<decimal, decimal>
    {
        public override decimal Evaluate(decimal value) => value * 2;
    }
}
