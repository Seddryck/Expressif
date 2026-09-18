using System.Text.Json;
using System.Text.Json.Nodes;

namespace Expressif.Cli.Configuration;

internal sealed class CliConfiguration(string path)
{
    public const string FileName = "expressif.config.json";
    public static readonly string[] Commands = ["repl", "run", "evaluate"];
    public static readonly string[] Settings = ["output-style", "indent", "preferred-line-width", "inline-types"];
    public static readonly string[] LineageSettings = ["url", "endpoint", "api-key", "namespace", "job-name", "disabled"];
    private static readonly HashSet<string> InlineTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "array", "tuple", "vector", "pair", "group", "record", "dictionary", "grouping",
    };
    public string Path { get; } = path;

    public static CliConfiguration CreateDefault() => new(System.IO.Path.Combine(AppContext.BaseDirectory, FileName));

    public string Get(string key)
    {
        var (command, setting) = ParseKey(key);
        var document = Read();
        if (command == "openlineage")
        {
            var environment = Environment.GetEnvironmentVariable(LineageEnvironment(setting));
            if (!string.IsNullOrWhiteSpace(environment))
                return ValidateLineageValue(setting, environment);
            return ReadLineageValue(document, setting);
        }
        var value = command is null ? null : (document[command] as JsonObject)?[setting];
        return ReadValue(HasValue(value) ? value : document[setting], setting);
    }

    public string GetSource(string key)
    {
        var (command, setting) = ParseKey(key);
        var document = Read();
        if (command == "openlineage")
        {
            var environment = LineageEnvironment(setting);
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(environment)))
                return environment;
            _ = ReadLineageValue(document, setting);
            return HasValue((document[command] as JsonObject)?[setting]) ? command + "." + setting : "built-in default";
        }
        if (command is not null && HasValue((document[command] as JsonObject)?[setting]))
            return command + "." + setting;
        return HasValue(document[setting]) ? setting : "built-in default";
    }

    public void Set(string key, string value)
    {
        var (command, setting) = ParseKey(key);
        var normalized = command == "openlineage" ? ValidateLineageValue(setting, value) : ValidateValue(setting, value);
        var document = Read();
        var target = document;
        if (command is not null)
        {
            if (document[command] is null)
                document[command] = new JsonObject();
            target = document[command] as JsonObject ?? throw new FormatException($"Configuration section '{command}' must be an object.");
        }

        target[setting] = command == "openlineage" && setting == "disabled"
            ? JsonValue.Create(bool.Parse(normalized))
            : setting switch
        {
            "indent" when normalized != "tab" => JsonValue.Create(ParseInteger(normalized)),
            "preferred-line-width" => JsonValue.Create(ParseInteger(normalized)),
            "inline-types" => CreateInlineTypes(normalized),
            _ => JsonValue.Create(normalized),
        };
        Write(document);
    }

    public void Unset(string key)
    {
        var (command, setting) = ParseKey(key);
        var document = Read();
        var target = command is null ? document : document[command] as JsonObject;
        if (target?.Remove(setting) == true)
            Write(document);
    }

    private static (string? Command, string Setting) ParseKey(string key)
    {
        var parts = key.Split('.');
        if (parts.Length == 1 && Settings.Contains(parts[0]))
            return (null, parts[0]);
        if (parts.Length == 2 && Commands.Contains(parts[0]) && Settings.Contains(parts[1]))
            return (parts[0], parts[1]);
        if (parts.Length == 2 && parts[0] == "openlineage" && LineageSettings.Contains(parts[1]))
            return (parts[0], parts[1]);
        throw new FormatException($"Unknown configuration key '{key}'.");
    }

    private static string ValidateValue(string setting, string value)
        => setting switch
        {
            "output-style" => ValidateOutputStyle(value),
            "indent" => ValidateIndent(value),
            "preferred-line-width" => ValidatePreferredLineWidth(value),
            "inline-types" => ValidateInlineTypes(value),
            _ => throw new FormatException($"Unknown configuration setting '{setting}'."),
        };

    private static string LineageEnvironment(string setting)
        => "OPENLINEAGE_" + setting.Replace('-', '_').ToUpperInvariant();

    private static string ReadLineageValue(JsonObject document, string setting)
    {
        if (document["openlineage"] is { } section && section is not JsonObject)
            throw new FormatException("Configuration section 'openlineage' must be an object.");
        var value = (document["openlineage"] as JsonObject)?[setting];
        if (!HasValue(value))
        {
            return setting switch
            {
                "endpoint" => "api/v1/lineage",
                "namespace" => "expressif",
                "disabled" => "false",
                _ => string.Empty,
            };
        }

        if (value is not JsonValue scalar || !(scalar.TryGetValue<string>(out _) || (setting == "disabled" && scalar.TryGetValue<bool>(out _))))
            throw new FormatException($"Configuration openlineage.{setting} has an invalid value type.");
        return ValidateLineageValue(setting, value!.ToString());
    }

    private static string ValidateLineageValue(string setting, string value)
    {
        if (setting == "disabled")
        {
            if (bool.TryParse(value, out var disabled))
                return disabled ? "true" : "false";
            throw new FormatException("Configuration openlineage.disabled must be true or false.");
        }
        if (string.IsNullOrWhiteSpace(value))
            throw new FormatException($"Configuration openlineage.{setting} must not be blank. Use config unset to remove it.");
        if (setting == "url" && (!Uri.TryCreate(value, UriKind.Absolute, out var url) || url.Scheme is not ("http" or "https")))
            throw new FormatException("Configuration openlineage.url must be an absolute HTTP or HTTPS URL.");
        if (setting == "endpoint" && (Uri.TryCreate(value.TrimStart('/'), UriKind.Absolute, out _) || value.StartsWith("//", StringComparison.Ordinal)))
            throw new FormatException("Configuration openlineage.endpoint must be a relative path.");
        return value;
    }

    private static string ValidateOutputStyle(string value)
    {
        if (value.ToLowerInvariant() is "compact" or "pretty")
            return value.ToLowerInvariant();

        throw new FormatException("Configuration output-style must be 'compact' or 'pretty'.");
    }

    private static string ValidateIndent(string value)
    {
        if (value.Equals("tab", StringComparison.OrdinalIgnoreCase))
            return "tab";
        if (int.TryParse(value, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var spaces)
            && spaces is >= 0 and <= 8)
        {
            return spaces.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        throw new FormatException("Configuration indent must be 'tab' or an integer from 0 to 8.");
    }

    private static string ValidatePreferredLineWidth(string value)
    {
        if (int.TryParse(value, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var width)
            && width > 0)
        {
            return width.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        throw new FormatException("Configuration preferred-line-width must be a positive integer.");
    }

    private static string ValidateInlineTypes(string value)
    {
        var types = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (types.Length == 1 && types[0].Equals("none", StringComparison.OrdinalIgnoreCase))
            return "none";
        if (types.Length > 0 && types.All(InlineTypes.Contains))
            return string.Join(',', types.Select(type => type.ToLowerInvariant()).Distinct());

        throw new FormatException(
            "Configuration inline-types must be 'none' or a comma-separated list of: array, tuple, vector, pair, group, record, dictionary, grouping.");
    }

    private static bool HasValue(JsonNode? value)
        => value is not null && !(value is JsonValue scalar
            && scalar.TryGetValue<string>(out var text) && string.IsNullOrWhiteSpace(text));

    private static string ReadValue(JsonNode? value, string setting)
    {
        if (!HasValue(value))
        {
            return setting switch
            {
                "indent" => "2",
                "preferred-line-width" => "80",
                "inline-types" => "tuple",
                _ => "compact",
            };
        }

        if (setting == "inline-types" && value is JsonArray types)
        {
            if (types.Count == 0)
                return "none";
            if (types.Any(type => type is not JsonValue scalar || !scalar.TryGetValue<string>(out _)))
                throw new FormatException("Configuration inline-types must be an array of type names.");
            return ValidateValue(setting, string.Join(',', types.Select(type => type!.GetValue<string>())));
        }

        return ValidateValue(setting, value!.ToString());
    }

    private JsonObject Read()
    {
        if (!File.Exists(Path))
            return new JsonObject();
        try
        {
            var content = File.ReadAllText(Path);
            if (string.IsNullOrWhiteSpace(content))
                return new JsonObject();
            var document = JsonNode.Parse(content) as JsonObject
                ?? throw new FormatException("Configuration must be a JSON object.");
            foreach (var setting in Settings)
            {
                _ = ReadValue(document[setting], setting);
                foreach (var command in Commands)
                {
                    if (document[command] is { } section)
                    {
                        if (section is not JsonObject values)
                            throw new FormatException($"Configuration section '{command}' must be an object.");
                        _ = ReadValue(values[setting], setting);
                    }
                }
            }

            return document;
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException)
        {
            throw new FormatException($"Invalid configuration JSON: {exception.Message}", exception);
        }
    }

    private static int ParseInteger(string value)
        => int.Parse(value, System.Globalization.CultureInfo.InvariantCulture);

    private static JsonArray CreateInlineTypes(string value)
        => value == "none"
            ? []
            : new JsonArray(value.Split(',').Select(type => (JsonNode?)JsonValue.Create(type)).ToArray());

    private void Write(JsonObject document)
    {
        var fullPath = System.IO.Path.GetFullPath(Path);
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fullPath)!);
        var temporary = fullPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temporary, document.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);
            File.Move(temporary, fullPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary))
                File.Delete(temporary);
        }
    }
}
