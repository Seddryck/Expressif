using Expressif.Values;
using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Expressif.Parsers;

public interface IParameter
{ }

public record class LiteralParameter (string Value) : IParameter { }
public record class IntervalParameter(Interval Value) : IParameter { }
public record class VariableParameter(string Name) : IParameter { }
public record class ObjectPropertyParameter(string Name) : IParameter { }
public record class ObjectIndexParameter(int Index) : IParameter { }
public record class ContextParameter(Func<IContext, object?> Function) : IParameter { }
public record class ArrayParameter(IParameter[] Values) : IParameter { }
public record class QuotedLiteralParameter(string Value) : IParameter { }
public record class IncomingValueParameter() : IParameter { }

public record class RecordLiteralField(string Name, IParameter Value);
public record class RecordLiteralParameter(RecordLiteralField[] Fields) : IParameter { }

public interface IRecordDefinitionEntry;
public record class RecordNamedEntry(string Name, IParameter Value) : IRecordDefinitionEntry;
public record class RecordSpreadEntry() : IRecordDefinitionEntry;
public record class RecordDefinitionParameter(IRecordDefinitionEntry[] Entries) : IParameter { }

public record class InputExpressionParameter(ClosedExpression Expression) : IParameter { }
public record class OpenExpressionParameter(OpenExpression Expression) : IParameter { }
public record class PredicationParameter(IPredication Predication) : IParameter { }

public class Parameter
{
    protected static readonly Parser<IParameter> VariableParameter =
        from name in Grammar.Variable
        select new VariableParameter(name);

    protected static readonly Parser<IParameter> ItemParameter =
        from _ in Parse.Char('[').Token()
        from name in Grammar.Literal
        from _1 in Parse.Char(']').Token()
        select new ObjectPropertyParameter(name);

    protected static readonly Parser<IParameter> IndexParameter =
        from _ in Parse.Char('#')
        from index in Parse.Number
        select new ObjectIndexParameter(int.Parse(index));

    protected static readonly Parser<IParameter> IncomingParameter =
        from _ in Parse.String("...").Token()
        select new IncomingValueParameter();

    protected static readonly Parser<IParameter> QuotedLiteralParameter =
        from value in Grammar.Quoted
        select new QuotedLiteralParameter(value);

    protected static readonly Parser<IParameter> ParametrizedExpressionParameter =
        from _ in Parse.Char('{').Token()
        from parameter in VariableParameter.Or(IndexParameter).Or(ItemParameter).Or(LiteralParameter).Token()
        from _0 in Parse.Char('|').Token()
        from expression in Parsers.OpenExpression.Parser
        from _1 in Parse.Char('}').Token()
        select new InputExpressionParameter(new ClosedExpression(parameter, expression.Members));

    protected static readonly Parser<IParameter> ArrayLiteralParameter =
        from _ in Parse.Char('{').Token()
        from values in (
            from close in Parse.Char('}').Token()
            select Array.Empty<IParameter>()
        ).Or(
            from first in Parse.Ref(() => RecordValueParameter).Token().Once()
            from others in (
                from __ in Parse.Char(',').Token()
                from p in Parse.Ref(() => RecordValueParameter).Token()
                select p
            ).Many()
            from close in Parse.Char('}').Token()
            select first.Concat(others).ToArray()
        )
        select new ArrayParameter(values);

    protected static readonly Parser<IParameter> RecordLiteralParameter =
        from _ in Parse.Char('{').Token()
        from fields in (
            from first in Parse.Ref(() => RecordLiteralFieldParser).Token().Once()
            from others in (
                from __ in Parse.Char(',').Token()
                from field in Parse.Ref(() => RecordLiteralFieldParser).Token()
                select field
            ).Many()
            from trailing in Parse.Char(',').Token().Optional()
            from close in Parse.Char('}').Token()
            select first.Concat(others).ToArray()
        )
        select new RecordLiteralParameter(fields);

    protected static readonly Parser<RecordLiteralField> RecordLiteralFieldParser =
        from name in Parse.Ref(() => RecordFieldNameParser)
        from _ in Parse.String(":=").Token()
        from value in Parse.Ref(() => RecordValueParameter).Token()
        select new RecordLiteralField(name, value);

    protected static readonly Parser<string> RecordFieldNameParser =
        Grammar.BareToken.Or(Grammar.Quoted);

    protected static readonly Parser<IParameter> RecordValueParameter =
        IncomingParameter
        .Or(Parse.Ref(() => RecordLiteralParameter))
        .Or(Parse.Ref(() => VariableParameter))
        .Or(Parse.Ref(() => IntervalParameter))
        .Or(Parse.Ref(() => IndexParameter))
        .Or(Parse.Ref(() => ItemParameter))
        .Or(QuotedLiteralParameter)
        .Or(Parse.Ref(() => LiteralParameter))
        ;

    protected static readonly Parser<IRecordDefinitionEntry> RecordSpreadEntry =
        from _ in Parse.String("...").Token()
        select new RecordSpreadEntry();

    protected static readonly Parser<IRecordDefinitionEntry> RecordNamedEntry =
        from name in RecordFieldNameParser
        from _ in Parse.String(":=").Token()
        from value in RecordAssignedValueParser.Token()
        select new RecordNamedEntry(name, value);

    protected static readonly Parser<IParameter> RecordAssignedValueParser =
        IncomingParameter
        .Or(OpenExpression.Parser.Select(x => (IParameter)new OpenExpressionParameter(x)))
        .Or(Parse.Ref(() => RecordValueParameter));

    public static readonly Parser<IRecordDefinitionEntry> RecordFunctionEntryParser =
        RecordSpreadEntry
        .Or(RecordNamedEntry);

    protected static readonly Parser<IParameter> LiteralParameter =
        from name in Grammar.Literal
        select new LiteralParameter(name);

    protected static readonly Parser<IParameter> IntervalParameter =
        from interval in Interval.Parser
        select new IntervalParameter(interval);

    public static readonly Parser<IParameter> Parser = 
        VariableParameter
        .Or(IntervalParameter)
        .Or(IndexParameter)
        .Or(ItemParameter)
        .Or(RecordLiteralParameter)
        .Or(ArrayLiteralParameter)
        .Or(ParametrizedExpressionParameter)
        .Or(LiteralParameter)
        ;
}

public class Parameters
{
    public static readonly Parser<IParameter[]> Parser =
        from _ in Parse.Char('(').Token()
        from first in Parameter.Parser.Once()
        from others in (
            from _ in Parse.Char(',').Token()
            from p in Parameter.Parser.Token()
            select p).Many()
        from _1 in Parse.Char(')').Token()
        select first.Concat(others).ToArray();
}
