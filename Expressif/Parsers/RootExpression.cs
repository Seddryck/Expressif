using Sprache;

namespace Expressif.Parsers;

public interface IRootExpression
{ }

public record class OpenRootExpression(OpenExpression Expression) : IRootExpression;

public record class ClosedRootExpression(ClosedExpression Expression) : IRootExpression;

public static class RootExpression
{
    private static readonly Parser<IRootExpression> TupleLiteralParser =
        from tuple in Parameter.TupleLiteralParser
        select (IRootExpression)new ClosedRootExpression(new ClosedExpression(tuple, []));

    public static readonly Parser<IRootExpression> Parser =
        TupleLiteralParser
        .Or(OpenExpression.Parser.Select(x => (IRootExpression)new OpenRootExpression(x)))
        .Or(ClosedExpression.Parser.Select(x => (IRootExpression)new ClosedRootExpression(x)))
        .End();
}
