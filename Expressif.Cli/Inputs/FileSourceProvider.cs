using Expressif.Cli.Infrastructure;

namespace Expressif.Cli.Inputs;

internal enum SourceFormat
{
    Csv,
    Json,
}

internal interface IFileSourceProvider
{
    SourceFormat? Format { get; }

    bool CanOpen(string path);

    object? Open(string path, IReadOnlyList<string> options);
}

internal sealed class CsvFileSourceProvider(SourceInfrastructure infrastructure) : IFileSourceProvider
{
    public SourceFormat? Format => SourceFormat.Csv;

    public bool CanOpen(string path)
        => Path.GetExtension(path).Equals(".csv", StringComparison.OrdinalIgnoreCase);

    public object? Open(string path, IReadOnlyList<string> options)
    {
        SourcePathValidator.Validate(path);
        return infrastructure.OpenCsvDataReader(path, options);
    }
}

internal sealed class JsonFileSourceProvider(SourceInfrastructure infrastructure) : IFileSourceProvider
{
    public SourceFormat? Format => SourceFormat.Json;

    public bool CanOpen(string path)
        => Path.GetExtension(path).Equals(".json", StringComparison.OrdinalIgnoreCase);

    public object? Open(string path, IReadOnlyList<string> options)
    {
        SourcePathValidator.Validate(path);
        return infrastructure.OpenJsonSource(path, options);
    }
}

internal sealed class ExpressionFileSourceProvider(SourceInfrastructure infrastructure) : IFileSourceProvider
{
    public SourceFormat? Format => null;

    public bool CanOpen(string path)
        => Path.GetExtension(path) is var extension
            && (extension.Equals(".expr", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".expressif", StringComparison.OrdinalIgnoreCase));

    public object? Open(string path, IReadOnlyList<string> options)
    {
        SourcePathValidator.Validate(path);
        return infrastructure.OpenExpressionSource(path, options);
    }
}

internal sealed class SourcePipeline(
    IReadOnlyList<IFileSourceProvider> providers,
    SourceInfrastructure infrastructure)
{
    public IEnumerable<object?> Read(
        string? sourcePath,
        IReadOnlyList<string> options,
        bool scalar = false,
        SourceFormat? format = null)
    {
        var path = sourcePath ?? string.Empty;
        object? source;
        try
        {
            var provider = format is null
                ? providers.FirstOrDefault(candidate => candidate.CanOpen(path))
                : providers.FirstOrDefault(candidate => candidate.Format == format);
            if (provider is null)
            {
                throw new FormatException(
                    $"The input format for source '{path}' could not be determined. Specify --format csv or --format json.");
            }
            source = provider.Open(path, options);
        }
        catch (FormatException)
        {
            throw;
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            throw new FormatException($"The source '{path}' could not be resolved: {exception.Message}", exception);
        }

        foreach (var row in infrastructure.Normalize(source, path, scalar))
            yield return row;
    }
}

internal static class SourcePathValidator
{
    public static void Validate(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new FormatException("Source path is required.");
        if (Directory.Exists(path))
            throw new FormatException($"Source '{path}' is a directory.");
        if (!File.Exists(path))
            throw new FormatException($"Source '{path}' was not found.");
    }
}
