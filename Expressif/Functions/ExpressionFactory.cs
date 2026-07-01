using Expressif.Parsers;
using Expressif.Functions.Array;
using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Expressif.Functions;

public class ExpressionFactory : BaseExpressionFactory
{
    private Parser<IRootExpression> Parser { get; } = RootExpression.Parser;
    private static readonly HashSet<string> ImplicitFoldAccumulators =
    [
        "count",
        "sum",
        "min",
        "max",
        "first",
        "last"
    ];

    public ExpressionFactory()
        : base(new FunctionTypeMapper()) { }

    public IFunction Instantiate(string code, IContext context)
    {
        var rootExpression = Parser.Parse(code);
        return rootExpression switch
        {
            OpenRootExpression open => BuildOpenExpression(open.Expression, context),
            ClosedRootExpression closed => BuildClosedExpression(closed.Expression, context),
            _ => throw new ParseException($"Unsupported expression root '{rootExpression.GetType().Name}'.")
        };
    }

    public IFunction Instantiate(string name, IParameter[] parameters, IContext context)
        => Instantiate<IFunction>(name, parameters, context);

    public IFunction Instantiate(Type type, IParameter[] parameters, IContext context)
        => Instantiate<IFunction>(type, parameters, context);

    private IFunction BuildOpenExpression(OpenExpression expression, IContext context)
    {
        var functions = new List<IFunction>();
        foreach (var member in expression.Members)
            functions.Add(InstantiateOrWrapAggregation(member, context));

        return new ChainFunction(functions);
    }

    private IFunction BuildClosedExpression(Parsers.ClosedExpression expression, IContext context)
    {
        var sourceParameter = CreateParameter(expression.Parameter, typeof(object), context);
        var functions = new List<IFunction>();
        foreach (var member in expression.Members)
            functions.Add(InstantiateOrWrapAggregation(member, context));

        return new DelegatedFunction(_ =>
        {
            var source = sourceParameter.DynamicInvoke();
            return functions.Aggregate(source, (current, function) => function.Evaluate(current));
        });
    }

    private IFunction InstantiateOrWrapAggregation(Parsers.Function function, IContext context)
    {
        var name = function.Name.ToKebabCase();

        if (name == "fold")
        {
            if (function.Parameters.Length != 1)
                throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);

            return new Fold(BuildAccumulatorNameProvider(function.Parameters[0], context));
        }

        if (name == "broadcast")
        {
            if (function.Parameters.Length != 1)
                throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);

            return new Broadcast(BuildAccumulatorNameProvider(function.Parameters[0], context));
        }

        if (ImplicitFoldAccumulators.Contains(name, StringComparer.OrdinalIgnoreCase) && function.Parameters.Length == 0)
            return new Fold(() => name);

        return Instantiate<IFunction>(function.Name, function.Parameters, context);
    }

    private Func<string> BuildAccumulatorNameProvider(IParameter parameter, IContext context)
    {
        var provider = CreateParameter(parameter, typeof(string), context);
        return () => provider.DynamicInvoke()?.ToString() ?? string.Empty;
    }

    private sealed class DelegatedFunction : IFunction
    {
        private Func<object?, object?> Function { get; }

        public DelegatedFunction(Func<object?, object?> function)
            => Function = function;

        public object? Evaluate(object? value)
            => Function.Invoke(value);
    }
}
