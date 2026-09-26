using System.Globalization;
using System.Text;
using System.Text.Json;
using Expressif.Functions.Catalog;

namespace Expressif.Planning;

/// <summary>
/// Reads and writes the portable Expressif logical-plan JSON format.
/// </summary>
public static class LogicalPlanJson
{
    public const string FormatName = "expressif.logical-plan";
    public const int FormatVersion = 1;
    public const string CatalogCompatibility = "3.0";
    public const string SchemaResourceName = "Expressif.LogicalPlan.schema.json";

    public static string Serialize(LogicalPlan plan, bool indented = true)
    {
        ArgumentNullException.ThrowIfNull(plan);
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = indented }))
        {
            writer.WriteStartObject();
            writer.WriteString("format", FormatName);
            writer.WriteNumber("version", FormatVersion);
            writer.WriteString("catalogCompatibility", CatalogCompatibility);
            writer.WritePropertyName("plan");
            WriteValue(writer, plan.Pipeline);
            writer.WriteEndObject();
        }
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    public static LogicalPlan Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = RequireObject(document.RootElement, "logical-plan document");
            EnsureProperties(root, "format", "version", "catalogCompatibility", "plan");
            var format = RequireString(root, "format");
            if (format != FormatName)
            {
                throw new LogicalPlanFormatException($"Unsupported logical-plan format '{format}'.");
            }
            var version = RequireInt32(root, "version");
            if (version != FormatVersion)
            {
                throw new LogicalPlanFormatException(
                    $"Unsupported logical-plan version '{version}'. This reader supports version '{FormatVersion}'.");
            }
            var compatibility = RequireString(root, "catalogCompatibility");
            if (compatibility != CatalogCompatibility)
            {
                throw new LogicalPlanFormatException(
                    $"Unsupported catalog compatibility '{compatibility}'. This reader supports '{CatalogCompatibility}'.");
            }
            var plan = ReadValue(RequireProperty(root, "plan"));
            return plan is LogicalPipeline pipeline
                ? new LogicalPlan(pipeline)
                : throw new LogicalPlanFormatException("The root plan node must be a pipeline.");
        }
        catch (JsonException exception)
        {
            throw new LogicalPlanFormatException("The logical-plan document is not valid JSON.", exception);
        }
        catch (Exception exception) when (exception is FormatException or InvalidOperationException or OverflowException)
        {
            throw new LogicalPlanFormatException("The logical-plan document contains an invalid value.", exception);
        }
    }

    public static string ReadSchema()
    {
        using var stream = typeof(LogicalPlanJson).Assembly.GetManifestResourceStream(SchemaResourceName)
            ?? throw new InvalidOperationException($"Embedded logical-plan schema '{SchemaResourceName}' was not found.");
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private static void WriteValue(Utf8JsonWriter writer, LogicalValue value)
    {
        writer.WriteStartObject();
        switch (value)
        {
            case LogicalPipeline pipeline:
                writer.WriteString("kind", "pipeline");
                writer.WritePropertyName("items");
                writer.WriteStartArray();
                foreach (var item in pipeline.Items)
                    WriteValue(writer, item);
                writer.WriteEndArray();
                break;
            case LogicalCall call:
                ValidateInputBinding(call);
                writer.WriteString("kind", "call");
                writer.WritePropertyName("operator");
                WriteFunction(writer, call.Function);
                writer.WriteNumber("contextDepth", call.ContextDepth);
                writer.WritePropertyName("arguments");
                writer.WriteStartArray();
                foreach (var argument in call.Arguments)
                    WriteArgument(writer, argument);
                writer.WriteEndArray();
                break;
            case LogicalLiteral literal:
                writer.WriteString("kind", "literal");
                writer.WriteString("type", literal.Type);
                writer.WritePropertyName("value");
                WriteLiteralValue(writer, literal);
                break;
            default:
                throw new LogicalPlanFormatException($"Unsupported logical-plan node '{value.GetType().Name}'.");
        }
        writer.WriteEndObject();
    }

    private static void WriteFunction(Utf8JsonWriter writer, PlannerFunctionDescriptor function)
    {
        writer.WriteStartObject();
        writer.WriteString("name", function.Name);
        writer.WriteString("input", function.Input);
        writer.WriteString("output", function.Output);
        if (function.Traversal is not null)
        {
            writer.WritePropertyName("traversal");
            writer.WriteStartObject();
            writer.WriteString("source", function.Traversal.Source);
            writer.WriteString("selection", function.Traversal.Selection);
            writer.WriteEndObject();
        }
        if (function.Semantics is not null)
        {
            writer.WritePropertyName("semantics");
            writer.WriteStartObject();
            writer.WriteString("cardinality", function.Semantics.Cardinality);
            writer.WriteString("dependency", function.Semantics.Dependency);
            writer.WriteString("ordering", function.Semantics.Ordering);
            writer.WriteEndObject();
        }
        writer.WriteEndObject();
    }

    private static void WriteArgument(Utf8JsonWriter writer, LogicalArgument argument)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("parameter");
        WriteParameter(writer, argument.Parameter);
        writer.WriteBoolean("explicit", argument.IsExplicit);
        writer.WriteBoolean("spread", argument.IsSpread);
        if (argument.Omission is not null)
        {
            writer.WritePropertyName("omission");
            WriteOmission(writer, argument.Omission);
        }
        if (argument.Value is not null)
        {
            writer.WritePropertyName("value");
            WriteValue(writer, argument.Value);
        }
        writer.WriteEndObject();
    }

    private static void WriteParameter(Utf8JsonWriter writer, PlannerParameterDescriptor parameter)
    {
        writer.WriteStartObject();
        writer.WriteString("name", parameter.Name);
        writer.WriteString("type", parameter.Type);
        writer.WriteBoolean("optional", parameter.Optional);
        writer.WriteBoolean("variadic", parameter.Variadic);
        writer.WriteNumber("minimumCardinality", parameter.MinimumCardinality);
        if (parameter.Evaluation is not null)
        {
            writer.WritePropertyName("evaluation");
            writer.WriteStartObject();
            writer.WriteString("frequency", parameter.Evaluation.Frequency);
            if (parameter.Evaluation.Source is not null)
                writer.WriteString("source", parameter.Evaluation.Source);
            if (parameter.Evaluation.Context is not null)
                writer.WriteString("context", parameter.Evaluation.Context);
            writer.WriteEndObject();
        }
        writer.WriteEndObject();
    }

    private static void WriteOmission(Utf8JsonWriter writer, ParameterOmissionDocumentation omission)
    {
        writer.WriteStartObject();
        writer.WriteString("mode", OmissionMode(omission.Mode));
        if (omission.Value.ValueKind != JsonValueKind.Undefined)
        {
            writer.WritePropertyName("value");
            omission.Value.WriteTo(writer);
        }
        if (omission.Source is not null)
            writer.WriteString("source", omission.Source);
        writer.WriteEndObject();
    }

    private static void WriteLiteralValue(Utf8JsonWriter writer, LogicalLiteral literal)
    {
        switch (literal.Value)
        {
            case null:
                writer.WriteNullValue();
                break;
            case bool boolean:
                writer.WriteBooleanValue(boolean);
                break;
            case string text:
                writer.WriteStringValue(text);
                break;
            case decimal number:
                writer.WriteNumberValue(number);
                break;
            case int number:
                writer.WriteNumberValue(number);
                break;
            case long number:
                writer.WriteNumberValue(number);
                break;
            case DateOnly date:
                writer.WriteStringValue(date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                break;
            case DateTime dateTime:
                writer.WriteStringValue(dateTime.ToString("O", CultureInfo.InvariantCulture));
                break;
            case TimeOnly time:
                writer.WriteStringValue(time.ToString("HH:mm:ss.fffffff", CultureInfo.InvariantCulture));
                break;
            case TimeSpan duration:
                writer.WriteStringValue(duration.ToString("c", CultureInfo.InvariantCulture));
                break;
            default:
                throw new LogicalPlanFormatException(
                    $"Literal type '{literal.Type}' contains unsupported value '{literal.Value.GetType().Name}'.");
        }
    }

    private static LogicalValue ReadValue(JsonElement element)
    {
        var value = RequireObject(element, "logical-plan node");
        return RequireString(value, "kind") switch
        {
            "pipeline" => ReadPipeline(value),
            "call" => ReadCall(value),
            "literal" => ReadLiteral(value),
            var kind => throw new LogicalPlanFormatException($"Unsupported logical-plan node kind '{kind}'."),
        };
    }

    private static LogicalPipeline ReadPipeline(JsonElement element)
    {
        EnsureProperties(element, "kind", "items");
        return new LogicalPipeline(RequireArray(element, "items").Select(ReadValue).ToArray());
    }

    private static LogicalCall ReadCall(JsonElement element)
    {
        EnsureProperties(element, "kind", "operator", "contextDepth", "arguments");
        var contextDepth = RequireInt32(element, "contextDepth");
        if (contextDepth < 0)
            throw new LogicalPlanFormatException("Property 'contextDepth' cannot be negative.");
        var call = new LogicalCall(
            ReadFunction(RequireProperty(element, "operator")),
            RequireArray(element, "arguments").Select(ReadArgument).ToArray(),
            contextDepth);
        ValidateInputBinding(call);
        return call;
    }

    private static void ValidateInputBinding(LogicalCall call)
    {
        if (!call.Function.Name.Equals("input-binding", StringComparison.Ordinal))
            return;
        if (call.ContextDepth != 0)
            throw new LogicalPlanFormatException("An input-binding call cannot declare a context depth.");
        if (!call.Arguments.Select(argument => argument.Parameter.Name)
                .SequenceEqual(new[] { "names", "positional", "body" }, StringComparer.Ordinal))
        {
            throw new LogicalPlanFormatException(
                "An input-binding call must contain names, positional, and body arguments in that order.");
        }
        if (call.Arguments.Any(argument => !argument.IsExplicit || argument.IsSpread || argument.Omission is not null))
            throw new LogicalPlanFormatException("Every input-binding argument must be explicit and cannot be spread.");

        var names = ReadInputBindingNames(call.Arguments[0].Value);
        if (call.Arguments[1].Value is not LogicalLiteral { Type: "boolean", Value: bool positional })
            throw new LogicalPlanFormatException("The input-binding positional argument must be a boolean literal.");
        if (call.Arguments[2].Value is not LogicalPipeline { Items.Count: > 0 })
            throw new LogicalPlanFormatException("The input-binding body argument must be a non-empty pipeline.");
        if (positional && names.Length < 2)
            throw new LogicalPlanFormatException("A positional input binding must declare at least two names.");
        if (!positional && names.Length > 1)
            throw new LogicalPlanFormatException("A named input binding cannot declare more than one name.");
        if (names.Distinct(StringComparer.Ordinal).Count() != names.Length)
            throw new LogicalPlanFormatException("An input binding cannot declare duplicate names.");
    }

    private static string[] ReadInputBindingNames(LogicalValue? value)
    {
        if (value is not LogicalCall { ContextDepth: 0 } names || names.Function.Name != "array")
            throw new LogicalPlanFormatException("The input-binding names argument must be an array call.");

        if (names.Arguments is
            [
                {
                    Parameter.Name: "values",
                    IsExplicit: false,
                    IsSpread: false,
                    Value: null,
                    Omission.Mode: ParameterOmissionMode.EmptyVariadic,
                },
            ])
        {
            return [];
        }

        var result = new List<string>();
        foreach (var argument in names.Arguments)
        {
            if (argument is not
                {
                    Parameter.Name: "values",
                    IsExplicit: true,
                    IsSpread: false,
                    Omission: null,
                    Value: LogicalLiteral { Type: "text", Value: string name },
                }
                || string.IsNullOrEmpty(name))
            {
                throw new LogicalPlanFormatException(
                    "The input-binding names array must contain explicit, non-empty text literals.");
            }
            result.Add(name);
        }
        return result.ToArray();
    }

    private static PlannerFunctionDescriptor ReadFunction(JsonElement element)
    {
        var function = RequireObject(element, "operator descriptor");
        EnsureProperties(function, "name", "input", "output", "traversal", "semantics");
        PlannerTraversalDescriptor? traversal = null;
        if (function.TryGetProperty("traversal", out var traversalElement))
        {
            var value = RequireObject(traversalElement, "traversal descriptor");
            EnsureProperties(value, "source", "selection");
            traversal = new PlannerTraversalDescriptor(
                RequireString(value, "source"),
                RequireString(value, "selection"));
        }
        var semantics = function.TryGetProperty("semantics", out var semanticsElement)
            ? ReadSemantics(semanticsElement)
            : null;
        return new PlannerFunctionDescriptor(
            RequireString(function, "name"),
            RequireString(function, "input"),
            RequireString(function, "output"),
            traversal,
            semantics);
    }

    private static LogicalArgument ReadArgument(JsonElement element)
    {
        var argument = RequireObject(element, "argument");
        EnsureProperties(argument, "parameter", "explicit", "spread", "omission", "value");
        var isExplicit = RequireBoolean(argument, "explicit");
        var isSpread = RequireBoolean(argument, "spread");
        var omission = argument.TryGetProperty("omission", out var omissionElement)
            ? ReadOmission(omissionElement)
            : null;
        var value = argument.TryGetProperty("value", out var valueElement) ? ReadValue(valueElement) : null;
        if (isExplicit == (value is null))
            throw new LogicalPlanFormatException("An explicit argument must contain a value and an omitted argument must not.");
        if (isExplicit == (omission is not null))
            throw new LogicalPlanFormatException("Only an omitted argument can contain omission metadata.");
        if (!isExplicit && isSpread)
            throw new LogicalPlanFormatException("An omitted argument cannot be spread.");
        var parameter = ReadParameter(RequireProperty(argument, "parameter"));
        if (!isExplicit && !parameter.Optional)
            throw new LogicalPlanFormatException("A required parameter cannot be omitted.");
        return new LogicalArgument(
            parameter,
            value,
            isSpread,
            isExplicit,
            omission);
    }

    private static PlannerParameterDescriptor ReadParameter(JsonElement element)
    {
        var parameter = RequireObject(element, "parameter descriptor");
        EnsureProperties(parameter, "name", "type", "optional", "variadic", "minimumCardinality", "evaluation");
        PlannerEvaluationDescriptor? evaluation = null;
        if (parameter.TryGetProperty("evaluation", out var evaluationElement))
        {
            var value = RequireObject(evaluationElement, "evaluation descriptor");
            EnsureProperties(value, "frequency", "source", "context");
            var frequency = RequireString(value, "frequency");
            if (frequency is not ("once" or "per-element" or "custom"))
                throw new LogicalPlanFormatException($"Unsupported evaluation frequency '{frequency}'.");
            evaluation = new PlannerEvaluationDescriptor(
                frequency,
                OptionalString(value, "source"),
                OptionalString(value, "context"));
        }
        var minimumCardinality = RequireInt32(parameter, "minimumCardinality");
        if (minimumCardinality < 0)
            throw new LogicalPlanFormatException("Property 'minimumCardinality' cannot be negative.");
        return new PlannerParameterDescriptor(
            RequireString(parameter, "name"),
            RequireString(parameter, "type"),
            RequireBoolean(parameter, "optional"),
            RequireBoolean(parameter, "variadic"),
            minimumCardinality,
            evaluation);
    }

    private static PlannerSemanticsDescriptor ReadSemantics(JsonElement element)
    {
        var semantics = RequireObject(element, "semantics descriptor");
        EnsureProperties(semantics, "cardinality", "dependency", "ordering");
        var cardinality = RequireString(semantics, "cardinality");
        if (cardinality is not ("preserved" or "non-increasing" or "collapsed" or "expanded" or "partitioned" or "unknown"))
            throw new LogicalPlanFormatException($"Unsupported semantics cardinality '{cardinality}'.");
        var dependency = RequireString(semantics, "dependency");
        if (dependency is not ("per-element" or "prefix" or "whole-input" or "partition" or "unknown"))
            throw new LogicalPlanFormatException($"Unsupported semantics dependency '{dependency}'.");
        var ordering = RequireString(semantics, "ordering");
        if (ordering is not ("preserved" or "reordered" or "unordered" or "not-applicable" or "unknown"))
            throw new LogicalPlanFormatException($"Unsupported semantics ordering '{ordering}'.");
        return new PlannerSemanticsDescriptor(cardinality, dependency, ordering);
    }

    private static ParameterOmissionDocumentation ReadOmission(JsonElement element)
    {
        var omission = RequireObject(element, "omission descriptor");
        EnsureProperties(omission, "mode", "value", "source");
        var mode = RequireString(omission, "mode") switch
        {
            "constant" => ParameterOmissionMode.Constant,
            "empty-variadic" => ParameterOmissionMode.EmptyVariadic,
            "absent" => ParameterOmissionMode.Absent,
            "environment-derived" => ParameterOmissionMode.EnvironmentDerived,
            var unsupported => throw new LogicalPlanFormatException($"Unsupported omission mode '{unsupported}'."),
        };
        var result = new ParameterOmissionDocumentation(
            mode,
            omission.TryGetProperty("value", out var value) ? value.Clone() : default,
            OptionalString(omission, "source"));
        ValidateOmission(result);
        return result;
    }

    private static void ValidateOmission(ParameterOmissionDocumentation omission)
    {
        var hasValue = omission.Value.ValueKind != JsonValueKind.Undefined;
        var hasSource = omission.Source is not null;
        var isValid = omission.Mode switch
        {
            ParameterOmissionMode.Constant => hasValue && !hasSource,
            ParameterOmissionMode.EmptyVariadic or ParameterOmissionMode.Absent => !hasValue && !hasSource,
            ParameterOmissionMode.EnvironmentDerived => !hasValue && hasSource,
            _ => false,
        };
        if (!isValid)
        {
            throw new LogicalPlanFormatException($"Omission metadata for mode '{OmissionMode(omission.Mode)}' is invalid.");
        }
    }

    private static LogicalLiteral ReadLiteral(JsonElement element)
    {
        EnsureProperties(element, "kind", "type", "value");
        var type = RequireString(element, "type");
        var value = RequireProperty(element, "value");
        object? semanticValue = type switch
        {
            "null" when value.ValueKind == JsonValueKind.Null => null,
            "boolean" => value.GetBoolean(),
            "text" or "type" => value.GetString(),
            "decimal" => value.GetDecimal(),
            "integer" => value.GetInt64(),
            "date" => DateOnly.ParseExact(value.GetString()!, "yyyy-MM-dd", CultureInfo.InvariantCulture),
            "datetime" => DateTime.Parse(value.GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            "time" => TimeOnly.Parse(value.GetString()!, CultureInfo.InvariantCulture),
            "duration" => TimeSpan.ParseExact(value.GetString()!, "c", CultureInfo.InvariantCulture),
            "all" when value.GetString() == "#all" => "#all",
            "ordering" when value.GetString() is "#less" or "#equal" or "#greater" => value.GetString(),
            _ => throw new LogicalPlanFormatException($"Unsupported or invalid literal type '{type}'."),
        };
        return new LogicalLiteral(type, semanticValue);
    }

    private static JsonElement RequireObject(JsonElement value, string description)
        => value.ValueKind == JsonValueKind.Object
            ? value
            : throw new LogicalPlanFormatException($"The {description} must be a JSON object.");

    private static JsonElement RequireProperty(JsonElement value, string name)
        => value.TryGetProperty(name, out var property)
            ? property
            : throw new LogicalPlanFormatException($"Required property '{name}' is missing.");

    private static IEnumerable<JsonElement> RequireArray(JsonElement value, string name)
    {
        var property = RequireProperty(value, name);
        if (property.ValueKind != JsonValueKind.Array)
            throw new LogicalPlanFormatException($"Property '{name}' must be an array.");
        return property.EnumerateArray();
    }

    private static string RequireString(JsonElement value, string name)
    {
        var property = RequireProperty(value, name);
        if (property.ValueKind != JsonValueKind.String)
            throw new LogicalPlanFormatException($"Property '{name}' must be a string.");
        var result = property.GetString()!;
        return result.Length > 0
            ? result
            : throw new LogicalPlanFormatException($"Property '{name}' cannot be empty.");
    }

    private static string? OptionalString(JsonElement value, string name)
        => value.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private static int RequireInt32(JsonElement value, string name)
        => RequireProperty(value, name).TryGetInt32(out var result)
            ? result
            : throw new LogicalPlanFormatException($"Property '{name}' must be a 32-bit integer.");

    private static bool RequireBoolean(JsonElement value, string name)
        => RequireProperty(value, name).ValueKind is JsonValueKind.True or JsonValueKind.False
            ? RequireProperty(value, name).GetBoolean()
            : throw new LogicalPlanFormatException($"Property '{name}' must be a boolean.");

    private static void EnsureProperties(JsonElement value, params string[] allowed)
    {
        var names = new HashSet<string>(allowed, StringComparer.Ordinal);
        foreach (var property in value.EnumerateObject())
        {
            if (!names.Contains(property.Name))
            {
                throw new LogicalPlanFormatException(
                    $"Property '{property.Name}' is not supported in version '{FormatVersion}'.");
            }
        }
    }

    private static string OmissionMode(ParameterOmissionMode mode) => mode switch
    {
        ParameterOmissionMode.Constant => "constant",
        ParameterOmissionMode.EmptyVariadic => "empty-variadic",
        ParameterOmissionMode.Absent => "absent",
        ParameterOmissionMode.EnvironmentDerived => "environment-derived",
        _ => throw new LogicalPlanFormatException($"Unsupported omission mode '{mode}'."),
    };
}

public sealed class LogicalPlanFormatException : Exception
{
    public LogicalPlanFormatException(string message)
        : base(message) { }

    public LogicalPlanFormatException(string message, Exception innerException)
        : base(message, innerException) { }
}
