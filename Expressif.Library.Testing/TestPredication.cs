using Expressif.Predicates;
using Expressif.Functions;

namespace Expressif.Testing;

internal static class TestPredication
{
    public static Predication Create(string text, IContext? context = null)
    {
        var evaluationContext = context ?? new Context();
        return new Predication(new PredicationFactory().Instantiate(text, evaluationContext));
    }
}

internal sealed class TestPredicationBuilder : PredicationBuilder
{
    public TestPredicationBuilder(IContext? context = null)
        : base(new FunctionFactory(TestExpression.LibraryTypeSource), context) { }
}
