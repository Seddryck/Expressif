using Expressif.Functions;
using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Expressif.Parsers;

public interface IExpression { }

public class OpenExpression : IExpression
{
    public IEnumerable<Function> Members { get; }

    public OpenExpression(IEnumerable<Function> members)
        => (Members) = (members);

    private static readonly Parser<Function> MapShorthandParser =
        from _ in Parse.String("|>").Token()
        from expression in Parse.Ref(() => Parser).Contained(Parse.Char('(').Token(), Parse.Char(')').Token())
        select new Function("map", [new OpenExpressionParameter(expression)], FunctionSyntax.MapShorthand);

    internal static readonly Parser<Function> ContinuationParser =
        MapShorthandParser.Or(
            from _ in Parse.Char('|').Token()
            from function in Function.Parser.Token()
            select function);

    public static readonly Parser<OpenExpression> Parser =
        from first in Function.Parser.Once()
        from others in ContinuationParser.Many()
        select new OpenExpression(first.Concat(others));
}

public class ClosedExpression : IExpression
{
    private static readonly HashSet<string> ImplicitFoldAccumulators =
    [
        "count",
        "sum",
        "min",
        "max",
        "first",
        "last"
    ];

    public IEnumerable<Function> Members { get; }
    public IParameter Parameter { get; }

    public ClosedExpression(IParameter parameter, IEnumerable<Function> members)
        => (Parameter, Members) = (parameter, members);

    public bool IsImplicitFoldAggregation
        => Members.Count() == 1
        && ImplicitFoldAccumulators.Contains(Members.First().Name, StringComparer.OrdinalIgnoreCase);

    public Function? GetImplicitFoldAccumulator()
        => IsImplicitFoldAggregation ? Members.First() : null;

    private static readonly Parser<ClosedExpression> AnyRootParser =
        from parameter in Parsers.Parameter.Parser.Token()
        from remaining in OpenExpression.ContinuationParser.Many()
        select new ClosedExpression(parameter, remaining);

    public static readonly Parser<ClosedExpression> Parser =
        AnyRootParser;
}

[Obsolete("Use OpenExpression instead.")]
public class Expression : OpenExpression
{
    public Expression(IEnumerable<Function> members)
        : base(members) { }

    public static new readonly Parser<Expression> Parser =
        from expression in OpenExpression.Parser
        select new Expression(expression.Members);
}

[Obsolete("Use ClosedExpression instead.")]
public class InputExpression : ClosedExpression
{
    public InputExpression(IParameter parameter, IEnumerable<Function> members)
        : base(parameter, members) { }

    public static new readonly Parser<InputExpression> Parser =
        from expression in ClosedExpression.Parser
        select new InputExpression(expression.Parameter, expression.Members);
}
