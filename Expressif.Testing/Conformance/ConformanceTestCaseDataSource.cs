using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Expressif.Testing.Conformance;

public static class ConformanceTestCaseDataSource
{
    public static IEnumerable<TestCaseData> GetCases(Type type, string testName)
    {
        Console.WriteLine($"Loading test case: for operator '{type.FullName}' and method '{testName}'");
        ArgumentNullException.ThrowIfNull(type);

        var fullName = type.FullName;
        if (string.IsNullOrWhiteSpace(fullName) || !fullName.StartsWith("Expressif.Testing.", StringComparison.Ordinal))
            throw new InvalidOperationException($"fullName must start with 'Expressif.Testing.'");


        var conformanceRoot = FindConformanceRoot();
        if (string.IsNullOrWhiteSpace(conformanceRoot))
            throw new InvalidOperationException($"Conformance root directory not found.");
        else
            Console.WriteLine($"\tFound conformance root directory: '{conformanceRoot}'");

        var relativeNamespace = fullName["Expressif.Testing.".Length..];
        var namespaceSegments = relativeNamespace.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (namespaceSegments.Length < 2)
            throw new InvalidOperationException($"Namespace '{relativeNamespace}' must have at least two segments.");

        var suiteDirectory = ToKebabCase(namespaceSegments[0]);
        var categoryDirectory = ToKebabCase(namespaceSegments[1]);
        var directory = Path.Combine(conformanceRoot, suiteDirectory, categoryDirectory);
        if (!Directory.Exists(directory))
            throw new InvalidOperationException($"Conformance directory '{directory}' not found.");
        else
            Console.WriteLine($"\tFound sub-directory: '{directory}'");

        var fileToken = testName.Split('_', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? testName;
        var expectedFile = Path.Combine(directory, ToKebabCase(fileToken) + ".yaml");
        if (!File.Exists(expectedFile))
            throw new FileNotFoundException($"Conformance YAML file '{expectedFile}' was not found for type '{fullName}'.", expectedFile);
        else
            Console.WriteLine($"\tFound conformance YAML file: '{expectedFile}'");

        var document = Deserialize(expectedFile);
        var test = document?.Tests?.Find(x => ToPascalSnakeCase(x.Id) == testName)
                        ?? throw new InvalidOperationException($"No test with id '{testName}' into the YAML conformance file");
        Console.WriteLine($"\tFound {document.Tests?.Count ?? 0} tests and {document.Tests?.Sum(x => x.Cases?.Count ?? 0) ?? 0} cases.");

        var method = type.GetMethod(testName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(fullName, testName);
        var parameters = method.GetParameters();
        var expectedParameterCount = parameters.Length;

        foreach (var @case in (test.Cases ?? throw new InvalidOperationException($"No cases named found for test '{testName}' into the YAML conformance file")))
        {
            var caseParameters = @case.Parameters ?? [];
            object?[] caseVariables = @case.Context?.Variables is null
                ? []
                : @case.Context.Variables.OrderBy(x => x.Key, StringComparer.Ordinal).Select(x => x.Value).ToArray();

            var parameterArgCount = caseParameters.Count;
            var rawArgs = new List<object?> { @case.Value };
            if (ShouldPackParametersIntoSingleArray(parameters, caseParameters, caseVariables))
            {
                rawArgs.Add(caseParameters.ToArray());
                parameterArgCount = 1;
            }
            else
            {
                rawArgs.AddRange(caseParameters);
            }

            rawArgs.AddRange(caseVariables);

            rawArgs.Add(@case.Expected);
            if (rawArgs.Count != expectedParameterCount)
            {
                throw new InvalidOperationException(
                    $"TestCaseSource argument count mismatch for '{fullName}.{testName}'. " +
                    $"Method expects {expectedParameterCount} parameter(s), but YAML produced {rawArgs.Count}. " +
                    $"YAML file: '{expectedFile}'.");
            }

            var args = rawArgs
                .Select((x, i) => Normalize(x, parameters[i].ParameterType))
                .ToArray();

            var parameterCount = parameterArgCount;
            var variableCount = caseVariables.Length;

            var input = args[0];
            var output = args[^1];
            var parameterValues = args.Skip(1).Take(parameterCount).ToArray();
            var variableValues = args.Skip(1 + parameterCount).Take(variableCount).ToArray();

            var displayParts = new List<string>
            {
                $"in: {FormatArgDisplay(input)}",
                $"out: {FormatArgDisplay(output)}"
            };
            if (parameterValues.Length > 0)
                displayParts.Add($"params: [{string.Join(", ", parameterValues.Select(FormatArgDisplay))}]");
            if (variableValues.Length > 0)
                displayParts.Add($"variables: [{string.Join(", ", variableValues.Select(FormatArgDisplay))}]");

            var argDisplayNames = new[]
            {
                string.Join(", ", displayParts)
            };

            yield return new TestCaseData(args)
                .SetArgDisplayNames(argDisplayNames);
        }
    }

    private static bool ShouldPackParametersIntoSingleArray(ParameterInfo[] methodParameters, IReadOnlyList<object?> caseParameters, IReadOnlyList<object?> caseVariables)
    {
        if (caseParameters.Count <= 1 || methodParameters.Length < 3)
            return false;

        var expandedCount = 1 + caseParameters.Count + caseVariables.Count + 1;
        if (expandedCount == methodParameters.Length)
            return false;

        if (!methodParameters[1].ParameterType.IsArray)
            return false;

        var packedCount = 1 + 1 + caseVariables.Count + 1;
        return packedCount == methodParameters.Length;
    }

    private static string? FindConformanceRoot()
    {
        foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory }
                     .Where(x => !string.IsNullOrWhiteSpace(x))
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var current = new DirectoryInfo(start)?.Parent?.Parent?.Parent?.Parent; // start one level above
            while (current is not null)
            {
                var candidate = Path.Combine(current.FullName, "conformance");
                if (Directory.Exists(candidate))
                    return candidate;

                current = current.Parent;
            }
        }

        return null;
    }

    private static YamlConformanceDocument? Deserialize(string path)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
        try
        {
            var yamlContent = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(yamlContent))
                throw new InvalidOperationException($"YAML file '{path}' is empty.");

            var doc = deserializer.Deserialize<YamlConformanceDocument>(yamlContent);
            return doc;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to read YAML file '{path}': {ex.Message}", ex);
        }
    }

    private static object? Normalize(object? value, Type expectedType)
    {
        var normalized = Normalize(value);
        if (normalized is null)
            return null;

        var targetType = Nullable.GetUnderlyingType(expectedType) ?? expectedType;
        if (targetType == typeof(object) || targetType.IsInstanceOfType(normalized))
            return normalized;

        try
        {
            if (targetType.IsArray)
            {
                var elementType = targetType.GetElementType()
                    ?? throw new InvalidOperationException($"Cannot resolve element type for array '{targetType.FullName}'.");

                if (normalized is not IEnumerable enumerable || normalized is string)
                    throw new InvalidOperationException($"Value '{FormatArg(normalized)}' cannot be converted to array type '{targetType.FullName}'.");

                var rawItems = enumerable.Cast<object?>().ToArray();
                var array = Array.CreateInstance(elementType, rawItems.Length);
                for (var i = 0; i < rawItems.Length; i++)
                {
                    var converted = Normalize(rawItems[i], elementType);
                    array.SetValue(converted, i);
                }

                return array;
            }

            if (targetType.IsEnum)
            {
                if (normalized is string enumText)
                    return Enum.Parse(targetType, enumText, true);

                var enumValue = Convert.ChangeType(normalized, Enum.GetUnderlyingType(targetType), CultureInfo.InvariantCulture);
                return Enum.ToObject(targetType, enumValue!);
            }

            if (targetType == typeof(char))
            {
                if (normalized is char c)
                    return c;
                if (normalized is string text && text.Length == 1)
                    return text[0];
                if (normalized is IConvertible)
                    return Convert.ToChar(normalized, CultureInfo.InvariantCulture);

                throw new InvalidOperationException($"Value '{FormatArg(normalized)}' cannot be converted to char.");
            }

            if (targetType == typeof(Guid) && normalized is string guidText)
                return Guid.Parse(guidText);

            if (targetType == typeof(DateOnly) && normalized is string dateOnlyText)
                return DateOnly.Parse(dateOnlyText, CultureInfo.InvariantCulture);

            if (targetType == typeof(TimeOnly) && normalized is string timeOnlyText)
                return TimeOnly.Parse(timeOnlyText, CultureInfo.InvariantCulture);

            var converter = TypeDescriptor.GetConverter(targetType);
            if (normalized is string stringValue && converter.CanConvertFrom(typeof(string)))
                return converter.ConvertFromInvariantString(stringValue);

            if (converter.CanConvertFrom(normalized.GetType()))
                return converter.ConvertFrom(null, CultureInfo.InvariantCulture, normalized);

            return Convert.ChangeType(normalized, targetType, CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Cannot convert value '{FormatArg(normalized)}' ({normalized.GetType().FullName}) to expected type '{expectedType.FullName}'.",
                ex);
        }
    }

    private static object? Normalize(object? value)
        => value switch
        {
            null => null,
            IDictionary<object, object> map => map.OrderBy(x => x.Key?.ToString(), StringComparer.Ordinal)
                .ToDictionary(x => x.Key?.ToString() ?? string.Empty, x => Normalize(x.Value), StringComparer.Ordinal),
            IDictionary dictionary => dictionary.Cast<DictionaryEntry>()
                .OrderBy(x => x.Key?.ToString(), StringComparer.Ordinal)
                .ToDictionary(x => x.Key?.ToString() ?? string.Empty, x => Normalize(x.Value), StringComparer.Ordinal),
            IEnumerable enumerable when value is not string
                => enumerable.Cast<object?>().Select(Normalize).ToArray(),
            _ => value,
        };

    private static string ToKebabCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var sb = new StringBuilder(value.Length + 8);
        for (var i = 0; i < value.Length;)
        {
            var c = value[i];
            if (char.IsUpper(c))
            {
                var start = i;
                while (i < value.Length && char.IsUpper(value[i]))
                    i++;

                var runLength = i - start;

                if (start > 0 && value[start - 1] != '-')
                    sb.Append('-');

                for (var j = 0; j < runLength; j++)
                {
                    if (j > 0 && j % 2 == 0)
                        sb.Append('-');

                    sb.Append(char.ToLowerInvariant(value[start + j]));
                }
            }
            else
            {
                sb.Append(c);
                i++;
            }
        }

        return sb.ToString();
    }

    public static string ToPascalSnakeCase(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return string.Join(
            "_",
            value
                .Split('-', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => char.ToUpperInvariant(part[0]) + part[1..])
        );
    }

    private static string FormatArg(object? value)
    {
        if (value is null)
            return "<null>";

        return value switch
        {
            string s => $"\"{s}\"",
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? value.ToString() ?? "<unknown>",
        };
    }

    private static string FormatArgDisplay(object? value)
    {
        if (value is null)
            return "<null>";

        return value switch
        {
            string s => $"{s}".Replace("(", "{").Replace(")", "}").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t"),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? value.ToString() ?? "<unknown>",
        };
    }

    private sealed class YamlConformanceDocument
    {
        public List<YamlConformanceTest>? Tests { get; set; }
    }

    private sealed class YamlConformanceTest
    {
        public required string Id { get; set; }
        public YamlTrace? Trace { get; set; }
        public List<YamlConformanceCase>? Cases { get; set; }
    }

    private sealed class YamlConformanceCase
    {
        public object? Value { get; set; }
        public List<object?>? Parameters { get; set; }
        public YamlContext? Context { get; set; }
        public object? Expected { get; set; }
    }

    private sealed class YamlContext
    {
        public Dictionary<string, object?>? Variables { get; set; }
    }

    private sealed class YamlTrace
    {
        public string? Class { get; set; }
        public string? File { get; set; }
        public string? QualifiedMethod { get; set; }
    }
}
