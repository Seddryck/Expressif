using Expressif.Bindings;
using Expressif.Functions.Accumulation;
using Expressif.Predicates;

namespace Expressif.Functions;

/// <summary>
/// Constructs a runtime function whose arguments require specialized evaluation.
/// </summary>
internal interface IFunctionConstructor
{
    IFunction Construct(
        Bindings.Function function,
        IContext context,
        IFunctionConstructionContext constructionContext);
}

/// <summary>
/// Associates a specialized constructor with a runtime function implementation.
/// </summary>
/// <typeparam name="TFunction">The runtime function implementation.</typeparam>
internal interface IFunctionConstructor<TFunction> : IFunctionConstructor
    where TFunction : IFunction
{ }

/// <summary>
/// Provides the generic expression-construction services needed by specialized constructors.
/// </summary>
internal interface IFunctionConstructionContext
{
    Delegate CreateParameter(IParameter parameter, Type targetType, IContext context);
    IFunction CreateOpenExpression(OpenExpression expression, IContext context);
    Func<object?, object?> CreateOpenExpressionValueEvaluator(
        OpenExpressionParameter expression,
        IContext context);
    Func<object?, object?> CreateValueEvaluator(
        IParameter parameter,
        IContext context,
        bool establishScope = false);
    IFunction CreateFunction(Bindings.Function function, IContext context);
    Func<IPredicate> CreatePredicateProvider(
        IParameter parameter,
        IContext context,
        string functionName);
    Func<IAccumulator> CreateAccumulatorProvider(IParameter parameter, IContext context);
    Func<IFunction> CreateTransformationProvider(
        OpenExpressionParameter parameter,
        IContext context);
    bool TryResolveImplementation(string name, out Type implementationType);
    Type ResolveTupleTarget(string name, Syntax.SourceSpan? sourceSpan = null);
    object? InvokeTuple(
        string name,
        Values.IPositionalValue tuple,
        Syntax.SourceSpan? sourceSpan = null);
}
