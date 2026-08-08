using Sprache;

namespace Expressif.Parsers;

public interface IRootExpression
{ }

public record class OpenRootExpression(OpenExpression Expression) : IRootExpression;

public record class ClosedRootExpression(ClosedExpression Expression) : IRootExpression;

public static class RootExpression
{
    private static readonly Parser<IRootExpression> LeadingMapPipelineParser =
        from map in OpenExpression.MapShorthandParser
        from others in OpenExpression.ContinuationParser.Many()
        select (IRootExpression)new OpenRootExpression(new OpenExpression([map, .. others]));

    private static readonly Parser<IRootExpression> TupleLiteralParser =
        from tuple in Parameter.TupleLiteralParser
        select (IRootExpression)new ClosedRootExpression(new ClosedExpression(tuple, []));

    public static readonly Parser<IRootExpression> Parser =
        LeadingMapPipelineParser
        .Or(TupleLiteralParser)
        .Or(OpenExpression.Parser.Select(x => (IRootExpression)new OpenRootExpression(x)))
        .Or(ClosedExpression.Parser.Select(x => (IRootExpression)new ClosedRootExpression(x)))
        .End();
}
