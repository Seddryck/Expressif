using System.Text.Json;
using System.Text.Json.Nodes;
using Expressif.Cli.Infrastructure;
using Expressif.Planning;

namespace Expressif.Cli.Commands;

internal sealed record ExpressionCommandSource(
    string Text,
    string? Code,
    LogicalPlan? Plan,
    string? Path)
{
    public bool IsPlan => Plan is not null;
    public bool IsFile => Path is not null;
}

internal static class ExpressionCommandSourceResolver
{
    public static bool TryResolve(
        string? inlineExpression,
        string? expressionFilePath,
        string? planFilePath,
        IStrictUtf8TextReader textReader,
        out ExpressionCommandSource source)
    {
        source = new ExpressionCommandSource(string.Empty, null, null, null);
        var hasInline = !string.IsNullOrWhiteSpace(inlineExpression);
        var hasExpressionFile = !string.IsNullOrWhiteSpace(expressionFilePath);
        var hasPlanFile = !string.IsNullOrWhiteSpace(planFilePath);
        if ((hasInline ? 1 : 0) + (hasExpressionFile ? 1 : 0) + (hasPlanFile ? 1 : 0) != 1)
        {
            Console.Error.WriteLine("The expression must be supplied through exactly one source: inline, --file, or --plan.");
            return false;
        }

        if (hasInline)
        {
            source = new ExpressionCommandSource(inlineExpression!, inlineExpression, null, null);
            return true;
        }

        var path = hasExpressionFile ? expressionFilePath! : planFilePath!;
        if (!TryRead(path, hasPlanFile, textReader, out var text))
            return false;

        if (hasExpressionFile)
        {
            source = new ExpressionCommandSource(text, text, null, path);
            return true;
        }

        try
        {
            source = new ExpressionCommandSource(text, null, DeserializePlan(text), path);
            return true;
        }
        catch (LogicalPlanFormatException exception)
        {
            Console.Error.WriteLine($"The plan loaded from '{path}' is invalid:");
            CommandDiagnosticWriter.WriteLine(exception.Message);
            return false;
        }
    }

    public static int WriteValidationError(Exception exception, ExpressionCommandSource source)
    {
        if (source.IsPlan)
        {
            Console.Error.WriteLine($"The plan loaded from '{source.Path}' is invalid:");
            CommandDiagnosticWriter.WriteLine(exception.Message);
            return ExitCodes.InvalidExpressionOrInput;
        }

        return ExpressionCommandCommon.WriteValidationError(
            exception, source.Code!, source.IsFile, source.Path);
    }

    private static bool TryRead(
        string path,
        bool plan,
        IStrictUtf8TextReader textReader,
        out string text)
    {
        text = string.Empty;
        try
        {
            text = textReader.Read(path);
            return true;
        }
        catch (TextFileReadException exception)
        {
            var kind = plan ? "Plan" : "Expression";
            var message = exception.Kind switch
            {
                TextFileFailureKind.Directory => $"{kind} file '{path}' is a directory.",
                TextFileFailureKind.NotFound => $"{kind} file '{path}' was not found.",
                TextFileFailureKind.InvalidUtf8 => $"{kind} file '{path}' could not be decoded as UTF-8.",
                TextFileFailureKind.Empty => $"{kind} file '{path}' is empty.",
                _ => $"{kind} file '{path}' could not be accessed: {exception.Message}",
            };
            Console.Error.WriteLine(message);
            return false;
        }
    }

    private static LogicalPlan DeserializePlan(string json)
    {
        JsonNode root;
        try
        {
            root = JsonNode.Parse(json)
                ?? throw new LogicalPlanFormatException("The plan document must be a JSON object.");
        }
        catch (JsonException exception)
        {
            throw new LogicalPlanFormatException("The plan document is not valid JSON.", exception);
        }

        if (root is not JsonObject document)
            throw new LogicalPlanFormatException("The plan document must be a JSON object.");
        if (document["format"] is not JsonValue formatValue
            || !formatValue.TryGetValue<string>(out var format)
            || string.IsNullOrWhiteSpace(format))
        {
            throw new LogicalPlanFormatException("Required property 'format' must be a non-empty string.");
        }

        return format switch
        {
            LogicalPlanJson.FormatName => LogicalPlanJson.Deserialize(json),
            AnalyzedLogicalPlanJson.FormatName => DeserializeAnalyzed(document),
            _ => throw new LogicalPlanFormatException($"Unsupported plan format '{format}'."),
        };
    }

    private static LogicalPlan DeserializeAnalyzed(JsonObject document)
    {
        EnsureProperties(document, "format", "version", "catalogCompatibility", "completeness", "diagnostics", "plan");
        var version = RequireInt32(document, "version");
        if (version != AnalyzedLogicalPlanJson.FormatVersion)
        {
            throw new LogicalPlanFormatException(
                $"Unsupported analyzed logical-plan version '{version}'. This reader supports version '{AnalyzedLogicalPlanJson.FormatVersion}'.");
        }

        var compatibility = RequireString(document, "catalogCompatibility");
        if (compatibility != LogicalPlanJson.CatalogCompatibility)
        {
            throw new LogicalPlanFormatException(
                $"Unsupported catalog compatibility '{compatibility}'. This reader supports '{LogicalPlanJson.CatalogCompatibility}'.");
        }

        _ = RequireString(document, "completeness");
        if (document["diagnostics"] is not JsonArray)
            throw new LogicalPlanFormatException("Property 'diagnostics' must be an array.");
        if (document["plan"] is not JsonObject analyzedPlan)
            throw new LogicalPlanFormatException("Property 'plan' must be an object.");

        var plan = analyzedPlan.DeepClone().AsObject();
        RemoveSchemaAnnotations(plan);
        var logical = new JsonObject
        {
            ["format"] = LogicalPlanJson.FormatName,
            ["version"] = LogicalPlanJson.FormatVersion,
            ["catalogCompatibility"] = LogicalPlanJson.CatalogCompatibility,
            ["definitions"] = new JsonArray(),
            ["plan"] = plan,
        };
        return LogicalPlanJson.Deserialize(logical.ToJsonString());
    }

    private static void RemoveSchemaAnnotations(JsonObject value)
    {
        if (value["kind"] is JsonValue kindValue
            && kindValue.TryGetValue<string>(out var kind)
            && kind is "pipeline" or "call" or "literal" or "named-expression-invocation")
        {
            value.Remove("input");
            value.Remove("output");
        }
        foreach (var child in value.Select(property => property.Value).OfType<JsonObject>())
            RemoveSchemaAnnotations(child);
        foreach (var array in value.Select(property => property.Value).OfType<JsonArray>())
        {
            foreach (var child in array.OfType<JsonObject>())
                RemoveSchemaAnnotations(child);
        }
    }

    private static void EnsureProperties(JsonObject value, params string[] allowed)
    {
        var names = new HashSet<string>(allowed, StringComparer.Ordinal);
        foreach (var property in value)
        {
            if (!names.Contains(property.Key))
                throw new LogicalPlanFormatException($"Property '{property.Key}' is not supported in an analyzed logical plan.");
        }
    }

    private static string RequireString(JsonObject value, string name)
        => value[name] is JsonValue property
            && property.TryGetValue<string>(out var result)
            && !string.IsNullOrEmpty(result)
                ? result
                : throw new LogicalPlanFormatException($"Property '{name}' must be a non-empty string.");

    private static int RequireInt32(JsonObject value, string name)
        => value[name] is JsonValue property && property.TryGetValue<int>(out var result)
            ? result
            : throw new LogicalPlanFormatException($"Property '{name}' must be a 32-bit integer.");
}
