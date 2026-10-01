using Expressif.Bindings;
using Expressif.Predicates;
using Expressif.Functions;
using Expressif.Library.Composition;
using Expressif.Syntax;
using Expressif.Values.Types;

namespace Expressif.Testing;

internal static class TestPredication
{
    public static Predication Create(string text, IContext? context = null)
    {
        var evaluationContext = context ?? new Context();
        var source = TestExpression.LibraryTypeSource;
        var plan = LogicalPlannerFactory.Create().Build(ExpressionParser.Parse(text));
        var bound = new LogicalPlanBinder(source, ExpressifTypeRegistry.Instance).Bind(plan);
        var function = new FunctionFactory(source).Instantiate(bound, evaluationContext);
        return new Predication(new BooleanFunctionPredicate(function));
    }
}

internal sealed class TestPredicationBuilder : PredicationBuilder
{
    public TestPredicationBuilder(IContext? context = null)
        : base(new FunctionFactory(TestExpression.LibraryTypeSource), context) { }
}
