using Expressif.Bindings;
using Expressif.Values;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Serializers;

public class ParameterSerializer
{
    private FunctionSerializer? functionSerializer;

    private FunctionSerializer FunctionSerializer => functionSerializer ??= new FunctionSerializer(this);

    public virtual string Serialize(IParameter parameter)
    {
        return parameter switch
        {
            ArrayParameter a => $"{{{string.Join(", ", a.Elements.Select(SerializeArrayElement))}}}",
            TupleParameter t => $"T({string.Join(", ", t.Values.Select(Serialize))})",
            RecordLiteralParameter r when r.Fields.Length == 0 => "{:}",
            RecordLiteralParameter r => $"{{{string.Join(", ", r.Fields.Select(x => $"{SerializeFieldName(x.Name)} := {Serialize(x.Value)}"))}}}",
            RecordDefinitionParameter definition => string.Join(", ", definition.Entries.Select(SerializeRecordEntry)),
            OpenExpressionParameter open => string.Join(" | ", open.Expression.Members.Select(FunctionSerializer.Serialize)),
            IncomingValueParameter => "...",
            QuotedLiteralParameter q => $"\"{RecordSyntax.EscapeDoubleQuoted(q.Value)}\"",
            LiteralParameter l => SerializeLiteral(l.Value),
            VariableParameter v => $"@{v.Name}",
            ObjectPropertyParameter op => $"[{op.Name}]",
            ObjectIndexParameter oi => $"#{oi.Index}",
            TupleProjectionParameter tp => tp.FromEnd ? $"$^{tp.Index}" : $"${tp.Index}",
            IntervalParameter interval => SerializeInterval(interval.Value),
            _ => throw new NotSupportedException()
        };
    }

    private string SerializeArrayElement(ArrayElementParameter element)
        => $"{(element.IsSpread ? "..." : string.Empty)}{Serialize(element.Value)}";

    private string SerializeRecordEntry(IRecordDefinitionEntry entry)
        => entry switch
        {
            RecordSpreadEntry => "...",
            RecordNamedEntry named => $"{SerializeFieldName(named.Name)} := {Serialize(named.Value)}",
            _ => throw new NotSupportedException(),
        };

    private static string SerializeFieldName(string name)
        => RecordSyntax.IsBareToken(name)
            ? name
            : $"\"{RecordSyntax.EscapeDoubleQuoted(name)}\"";

    private static string SerializeLiteral(object? value)
        => value switch
        {
            null => "#null",
            bool boolean => boolean ? "#true" : "#false",
            decimal numeric => numeric.ToString(CultureInfo.InvariantCulture),
            DateOnly date => $"#\"{date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}\"",
            DateTime dateTime => $"#\"{dateTime.ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture)}\"",
            TimeOnly time => $"#\"{time.ToString("HH:mm:ss", CultureInfo.InvariantCulture)}\"",
            string text when RecordSyntax.IsBareToken(text) => text,
            string text => $"\"{RecordSyntax.EscapeDoubleQuoted(text)}\"",
            _ => throw new NotSupportedException($"Literal value type '{value.GetType().Name}' cannot be serialized."),
        };

    private static string SerializeInterval(IntervalBinding interval)
        => $"I{(interval.IsLowerInclusive ? '[' : '(')}{SerializeBound(interval.LowerBound)}, {SerializeBound(interval.UpperBound)}{(interval.IsUpperInclusive ? ']' : ')')}";

    private static string SerializeBound(IntervalBoundBinding bound) => bound.Kind switch
    {
        IntervalBoundBindingKind.NegativeInfinity => "-INF",
        IntervalBoundBindingKind.PositiveInfinity => "+INF",
        IntervalBoundBindingKind.Finite => SerializeLiteral(bound.Value),
        _ => throw new NotSupportedException($"Interval bound kind '{bound.Kind}' cannot be serialized."),
    };
}
