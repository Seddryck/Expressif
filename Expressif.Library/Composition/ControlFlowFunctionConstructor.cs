using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal sealed class ControlFlowFunctionConstructor :
    IFunctionConstructor<Expressif.Library.Flow.Switch>,
    IFunctionConstructor<Expressif.Library.Flow.Try>
{
    public IFunction Construct(
        Bindings.Function function,
        IContext context,
        IFunctionConstructionContext constructionContext)
    {
        var branches = function.Parameters.Cast<ControlFlowBranchParameter>()
            .Select(branch => new Expressif.Library.Flow.ControlFlowBranch(
                ConditionalFunctionConstructor.BuildEvaluator(
                    branch.Expression, context, constructionContext),
                branch.Predicate is null
                    ? null
                    : ConditionalFunctionConstructor.BuildEvaluator(
                        branch.Predicate, context, constructionContext)))
            .ToArray();
        return function.Name.Equals("try", StringComparison.OrdinalIgnoreCase)
            ? new Expressif.Library.Flow.Try(branches)
            : new Expressif.Library.Flow.Switch(branches);
    }
}
