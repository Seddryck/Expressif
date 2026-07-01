using Expressif.Parsers;
using Expressif.Functions.Array;
using Sprache;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Expressif.Functions;

public class ExpressionFactory : BaseExpressionFactory
{
    private Parser<Parsers.Expression> Parser { get; } = Parsers.Expression.Parser;

    public ExpressionFactory()
        : base(new FunctionTypeMapper()) { }

    public IFunction Instantiate(string code, IContext context)
    {
        try
        {
            var expression = Parser.Parse(code);

            var functions = new List<IFunction>();
            foreach (var member in expression.Members)
                functions.Add(Instantiate<IFunction>(member.Name, member.Parameters, context));
            return new ChainFunction(functions);
        }
        catch (ParseException)
        {
            var inputExpression = InputExpression.Parser.Parse(code);
            if (!inputExpression.IsImplicitFoldAggregation)
                throw;

            var accumulatorFunction = inputExpression.GetImplicitFoldAccumulator()!;
            var sourceParameter = CreateParameter(inputExpression.Parameter, typeof(object), context);

            return new DelegatedFunction(_ =>
            {
                var source = sourceParameter.DynamicInvoke();
                var fold = new Fold(() => InstantiateAccumulator(accumulatorFunction.Name, accumulatorFunction.Parameters, context));
                return fold.Evaluate(source);
            });
        }
    }

    public IFunction Instantiate(string name, IParameter[] parameters, IContext context)
        => Instantiate<IFunction>(name, parameters, context);

    public IFunction Instantiate(Type type, IParameter[] parameters, IContext context)
        => Instantiate<IFunction>(type, parameters, context);

    private IAccumulator InstantiateAccumulator(string name, IParameter[] parameters, IContext context)
    {
        var type = name.ToKebabCase() switch
        {
            "count" => typeof(CountAccumulator),
            "sum" => typeof(SumAccumulator),
            "min" => typeof(MinAccumulator),
            "max" => typeof(MaxAccumulator),
            "first" => typeof(FirstAccumulator),
            "last" => typeof(LastAccumulator),
            _ => throw new NotImplementedFunctionException(name),
        };

        return Instantiate<IAccumulator>(type, parameters, context);
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
