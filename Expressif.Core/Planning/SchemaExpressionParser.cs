namespace Expressif.Planning;

internal static class SchemaExpressionParser
{
    public static SchemaExpression Parse(string text)
    {
        var index = 0;
        var expression = Parse(text, ref index);
        SkipWhitespace(text, ref index);
        if (index != text.Length)
            throw new InvalidOperationException($"Invalid schema expression '{text}' at position {index}.");
        return expression;
    }

    private static SchemaExpression Parse(string text, ref int index)
    {
        SkipWhitespace(text, ref index);
        var start = index;
        while (index < text.Length && (char.IsLetterOrDigit(text[index]) || text[index] is '-' or '_'))
            index++;
        if (start == index)
            throw new InvalidOperationException($"Invalid schema expression '{text}' at position {index}.");
        var name = text[start..index];
        SkipWhitespace(text, ref index);
        if (index >= text.Length || text[index] != '<')
            return new SchemaExpression(name, []);
        index++;
        var arguments = new List<SchemaExpression>();
        while (true)
        {
            arguments.Add(Parse(text, ref index));
            SkipWhitespace(text, ref index);
            if (index < text.Length && text[index] == ',')
            {
                index++;
                continue;
            }
            if (index >= text.Length || text[index] != '>')
                throw new InvalidOperationException($"Invalid schema expression '{text}' at position {index}.");
            index++;
            return new SchemaExpression(name, arguments);
        }
    }

    private static void SkipWhitespace(string text, ref int index)
    {
        while (index < text.Length && char.IsWhiteSpace(text[index]))
            index++;
    }
}
