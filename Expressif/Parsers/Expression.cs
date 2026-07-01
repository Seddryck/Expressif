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

    public static readonly Parser<OpenExpression> Parser =
        from first in Function.Parser.Once()
        from others in (
            from _ in Parse.Char('|').Token()
            from p in Function.Parser.Token()
            select p).Many()
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

    private static readonly Parser<IParameter> NonLiteralRootParameterParser =
        Parsers.Parameter.Parser.Where(x => x is not LiteralParameter);

    private static readonly Parser<ClosedExpression> NonLiteralRootParser =
        from parameter in NonLiteralRootParameterParser.Token()
        from remaining in (
            from _ in Parse.Char('|').Token()
            from expression in OpenExpression.Parser
            select expression.Members
        ).Optional()
        select new ClosedExpression(parameter, remaining.GetOrElse(Enumerable.Empty<Function>()));

    private static readonly Parser<ClosedExpression> LiteralRootOnlyParser =
        from parameter in Parsers.Parameter.Parser.Where(x => x is LiteralParameter).Token()
        select new ClosedExpression(parameter, Enumerable.Empty<Function>());

    public static readonly Parser<ClosedExpression> Parser =
        NonLiteralRootParser.Or(LiteralRootOnlyParser);
}

[Obsolete("Use OpenExpression instead.")]
public class Expression : OpenExpression
{
    public Expression(IEnumerable<Function> members)
        : base(members) { }

    public static readonly Parser<Expression> Parser =
        from expression in OpenExpression.Parser
        select new Expression(expression.Members);
}

[Obsolete("Use ClosedExpression instead.")]
public class InputExpression : ClosedExpression
{
    public InputExpression(IParameter parameter, IEnumerable<Function> members)
        : base(parameter, members) { }

    public static readonly Parser<InputExpression> Parser =
        from expression in ClosedExpression.Parser
        select new InputExpression(expression.Parameter, expression.Members);
}
