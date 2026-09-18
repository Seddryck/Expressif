using Expressif.Observability;
using Expressif.OpenLineage;
using Expressif.Cli.Inputs;
using Expressif.Cli.Configuration;

namespace Expressif.Cli.Application;

internal static class CliLineage
{
    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(5) };

    public static IExpressionObservation Begin(string expression, string command, string? sourcePath = null, SourceFormat? format = null, CliConfiguration? configuration = null)
    {
        try
        {
            configuration ??= CliConfiguration.CreateDefault();
            if (configuration.Get("openlineage.disabled") == "true")
                return NoOpExpressionObserver.Instance.Begin(ExpressionObservationStage.Evaluate);
            var url = configuration.Get("openlineage.url");
            if (string.IsNullOrWhiteSpace(url))
                return NoOpExpressionObserver.Instance.Begin(ExpressionObservationStage.Evaluate);
            var inputs = IsDataFile(sourcePath, format)
                ? new[] { OpenLineageDataset.FromFile(sourcePath!) }
                : [];
            var options = new OpenLineageOptions
            {
                Expression = expression,
                Namespace = configuration.Get("openlineage.namespace"),
                JobName = configuration.Get("openlineage.job-name") is { Length: > 0 } name ? name : command,
                Inputs = inputs,
            };
            var transport = new HttpOpenLineageTransport(Client, new Uri(url),
                configuration.Get("openlineage.endpoint"), configuration.Get("openlineage.api-key"));
            return new OpenLineageObserver(options, transport, Report).Begin(ExpressionObservationStage.Evaluate);
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            Report(exception);
            return NoOpExpressionObserver.Instance.Begin(ExpressionObservationStage.Evaluate);
        }
    }

    private static bool IsDataFile(string? path, SourceFormat? format)
        => !string.IsNullOrWhiteSpace(path) && (format is not null
            || Path.GetExtension(path).Equals(".json", StringComparison.OrdinalIgnoreCase)
            || Path.GetExtension(path).Equals(".csv", StringComparison.OrdinalIgnoreCase));

    private static void Report(Exception exception)
        => Console.Error.WriteLine($"OpenLineage reporting failed: {exception.Message}");
}
