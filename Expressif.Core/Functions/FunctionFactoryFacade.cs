using System.Reflection;
using Expressif.Bindings;
using Expressif.Discovery;
using Expressif.Predicates;
using Expressif.Values;

namespace Expressif.Functions;

/// <summary>Constructs executable functions from bound expressions.</summary>
public sealed class FunctionFactory
{
    private readonly FunctionFactoryRuntime runtime;

    public FunctionFactory(ITypeSource source)
        => runtime = new FunctionFactoryRuntime(source);

    internal FunctionFactory(IImplementationRegistry registry, ITypeSource source)
        => runtime = new FunctionFactoryRuntime(registry, source);

    public IFunction Instantiate(IRootExpression rootExpression, IContext context)
        => runtime.Instantiate(rootExpression, context);

    public IFunction InstantiateClosed(IRootExpression rootExpression, IContext context)
        => runtime.InstantiateClosed(rootExpression, context);

    internal IFunction Instantiate(string name, IParameter[] parameters, IContext context)
        => runtime.Instantiate(name, parameters, context);

    internal IFunction Instantiate(Type type, IParameter[] parameters, IContext context)
        => runtime.Instantiate(type, parameters, context);

    internal IFunction Instantiate(OpenExpression expression, IContext context)
        => runtime.Instantiate(expression, context);

    internal IPredicate InstantiatePredication(IPredication predication, IContext context)
        => runtime.InstantiatePredication(predication, context);

    internal bool TryBuildTypedChain(
        IReadOnlyList<Bindings.Function> members,
        List<IFunction> functions,
        out IFunction? chain)
        => runtime.TryBuildTypedChain(members, functions, out chain);

    internal ConstructorInfo GetMatchingConstructor(Type type, int parameterCount)
        => runtime.GetMatchingConstructor(type, parameterCount);

    internal object? InvokeTuple(string name, IPositionalValue tuple)
        => runtime.InvokeTuple(name, tuple);

    internal static bool IsExplicitlyRooted(OpenExpressionParameter expression)
        => FunctionFactoryRuntime.IsExplicitlyRooted(expression);

    internal static bool IsExplicitlyRooted(IParameter parameter)
        => FunctionFactoryRuntime.IsExplicitlyRooted(parameter);
}
