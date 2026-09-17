using Expressif.Observability;
using Expressif.OpenLineage;
using Expressif.Cli.Inputs;

namespace Expressif.Cli.Application;

internal static class CliLineage
{
    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(5) };

    public static IExpressionObservation Begin(string expression, string command, string? sourcePath = null, SourceFormat? format = null)
    {
        var url = Environment.GetEnvironmentVariable("OPENLINEAGE_URL");
        if (string.IsNullOrWhiteSpace(url) || string.Equals(Environment.GetEnvironmentVariable("OPENLINEAGE_DISABLED"), "true", StringComparison.OrdinalIgnoreCase))
            return NoOpExpressionObserver.Instance.Begin(ExpressionObservationStage.Evaluate);

        try
        {
            var inputs = IsDataFile(sourcePath, format)
                ? new[] { OpenLineageDataset.FromFile(sourcePath!) }
                : [];
            var options = new OpenLineageOptions
            {
                Expression = expression,
                Namespace = Setting("OPENLINEAGE_NAMESPACE", "expressif"),
                JobName = Setting("OPENLINEAGE_JOB_NAME", command),
                Inputs = inputs,
            };
            var transport = new HttpOpenLineageTransport(Client, new Uri(url),
                Setting("OPENLINEAGE_ENDPOINT", "api/v1/lineage"), Environment.GetEnvironmentVariable("OPENLINEAGE_API_KEY"));
            return new OpenLineageObserver(options, transport, Report).Begin(ExpressionObservationStage.Evaluate);
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            Report(exception);
            return NoOpExpressionObserver.Instance.Begin(ExpressionObservationStage.Evaluate);
        }
    }

    private static string Setting(string name, string fallback)
        => Environment.GetEnvironmentVariable(name) is { } value && !string.IsNullOrWhiteSpace(value) ? value : fallback;

    private static bool IsDataFile(string? path, SourceFormat? format)
        => !string.IsNullOrWhiteSpace(path) && (format is not null
            || Path.GetExtension(path).Equals(".json", StringComparison.OrdinalIgnoreCase)
            || Path.GetExtension(path).Equals(".csv", StringComparison.OrdinalIgnoreCase));

    private static void Report(Exception exception)
        => Console.Error.WriteLine($"OpenLineage reporting failed: {exception.Message}");
}
