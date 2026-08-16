using Expressif.Bindings;
using Expressif.Values;
using System;
using System.Collections.Generic;
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
            ArrayParameter a => $"{{{string.Join(", ", a.Values.Select(Serialize))}}}",
            TupleParameter t => $"T({string.Join(", ", t.Values.Select(Serialize))})",
            RecordLiteralParameter r when r.Fields.Length == 0 => "{:}",
            RecordLiteralParameter r => $"{{{string.Join(", ", r.Fields.Select(x => $"{SerializeFieldName(x.Name)} := {Serialize(x.Value)}"))}}}",
            RecordDefinitionParameter definition => string.Join(", ", definition.Entries.Select(SerializeRecordEntry)),
            OpenExpressionParameter open => string.Join(" | ", open.Expression.Members.Select(FunctionSerializer.Serialize)),
            IncomingValueParameter => "...",
            QuotedLiteralParameter q => $"\"{RecordSyntax.EscapeDoubleQuoted(q.Value)}\"",
            LiteralParameter l => RecordSyntax.IsBareToken(l.Value)
                ? l.Value
                : $"\"{RecordSyntax.EscapeDoubleQuoted(l.Value)}\"",
            VariableParameter v => $"@{v.Name}",
            ObjectPropertyParameter op => $"[{op.Name}]",
            ObjectIndexParameter oi => $"#{oi.Index}",
            TupleProjectionParameter tp => $"${tp.Index}",
            _ => throw new NotSupportedException()
        };
    }

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
}
