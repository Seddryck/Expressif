using System.Text.Json;
using System.Text.Json.Nodes;

namespace Expressif.Cli.Configuration;

internal sealed class CliConfiguration(string path)
{
    public const string FileName = "expressif.config.json";
    public static readonly string[] Commands = ["repl", "run", "evaluate"];
    public string Path { get; } = path;

    public static CliConfiguration CreateDefault() => new(System.IO.Path.Combine(AppContext.BaseDirectory, FileName));

    public string Get(string key)
    {
        var (command, setting) = ParseKey(key);
        var document = Read();
        var value = command is null ? null : (document[command] as JsonObject)?[setting];
        return ReadValue(value ?? document[setting], setting);
    }

    public string GetSource(string key)
    {
        var (command, setting) = ParseKey(key);
        var document = Read();
        if (command is not null && (document[command] as JsonObject)?[setting] is not null)
            return command + "." + setting;
        return document[setting] is not null ? setting : "built-in default";
    }

    public void Set(string key, string value)
    {
        var (command, setting) = ParseKey(key);
        var normalized = ValidateValue(setting, value);
        var document = Read();
        var target = document;
        if (command is not null)
        {
            if (document[command] is null)
                document[command] = new JsonObject();
            target = document[command] as JsonObject ?? throw new FormatException($"Configuration section '{command}' must be an object.");
        }

        target[setting] = setting == "indent" && normalized != "tab"
            ? JsonValue.Create(int.Parse(normalized, System.Globalization.CultureInfo.InvariantCulture))
            : JsonValue.Create(normalized);
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
        if (parts.Length == 1 && parts[0] is "output-style" or "indent")
            return (null, parts[0]);
        if (parts.Length == 2 && Commands.Contains(parts[0]) && parts[1] is "output-style" or "indent")
            return (parts[0], parts[1]);
        throw new FormatException($"Unknown configuration key '{key}'.");
    }

    private static string ValidateValue(string setting, string value)
    {
        if (setting == "output-style" && value.ToLowerInvariant() is "compact" or "pretty")
            return value.ToLowerInvariant();
        if (setting == "indent")
        {
            if (value.Equals("tab", StringComparison.OrdinalIgnoreCase))
                return "tab";
            if (int.TryParse(value, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var spaces)
                && spaces is >= 0 and <= 8)
                return spaces.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        throw new FormatException(setting == "indent"
            ? "Configuration indent must be 'tab' or an integer from 0 to 8."
            : "Configuration output-style must be 'compact' or 'pretty'.");
    }

    private static string ReadValue(JsonNode? value, string setting)
        => value is null ? setting == "indent" ? "2" : "compact" : ValidateValue(setting, value.ToString());

    private JsonObject Read()
    {
        if (!File.Exists(Path))
            return new JsonObject();
        try
        {
            var document = JsonNode.Parse(File.ReadAllText(Path)) as JsonObject
                ?? throw new FormatException("Configuration must be a JSON object.");
            foreach (var setting in new[] { "output-style", "indent" })
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
