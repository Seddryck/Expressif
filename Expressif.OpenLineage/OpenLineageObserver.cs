using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Expressif.Observability;

namespace Expressif.OpenLineage;

/// <summary>
/// Reports evaluation operations through the existing expression observer API.
/// </summary>
/// <remarks>
/// Parse and bind scopes are ignored. Each evaluation has an independent run ID.
/// Transport failures are isolated from execution and can be reported through the diagnostic callback.
/// </remarks>
public sealed class OpenLineageObserver : IExpressionObserver
{
    public const string SchemaUrl = "https://openlineage.io/spec/2-0-2/OpenLineage.json#/$defs/RunEvent";
    public const string FacetSchemaUrl = "https://openlineage.io/spec/2-0-2/OpenLineage.json#/$defs/BaseFacet";
    private static readonly string Version = typeof(Expression).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? typeof(Expression).Assembly.GetName().Version?.ToString() ?? "unknown";
    private static readonly string Producer = "https://github.com/Seddryck/Expressif";
    private readonly IOpenLineageTransport transport;
    private readonly Action<Exception>? diagnostic;
    private readonly object job;
    private readonly object[] inputs;
    private readonly object[] outputs;

    public OpenLineageObserver(OpenLineageOptions options, IOpenLineageTransport transport, Action<Exception>? diagnostic = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(transport);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Namespace);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.JobName);
        ArgumentNullException.ThrowIfNull(options.Expression);
        this.transport = transport;
        this.diagnostic = diagnostic;
        job = new
        {
            @namespace = options.Namespace,
            name = options.JobName,
            facets = new
            {
                expressif_expression = new
                {
                    _producer = Producer,
                    _schemaURL = FacetSchemaUrl,
                    expression = options.Expression,
                    expressifVersion = Version,
                    expressionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(options.Expression))).ToLowerInvariant(),
                },
            },
        };
        inputs = Datasets(options.Inputs);
        outputs = Datasets(options.Outputs);
    }

    public IExpressionObservation Begin(ExpressionObservationStage stage)
        => stage == ExpressionObservationStage.Evaluate
            ? new RunObservation(this)
            : NoOpExpressionObserver.Instance.Begin(stage);

    private static object[] Datasets(IReadOnlyList<OpenLineageDataset> datasets)
        => datasets.Select(dataset =>
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(dataset.Namespace);
            ArgumentException.ThrowIfNullOrWhiteSpace(dataset.Name);
            return (object)new { @namespace = dataset.Namespace, name = dataset.Name };
        }).ToArray();

    private void Emit(Guid runId, string eventType)
    {
        try
        {
            transport.Emit(JsonSerializer.Serialize(new
            {
                eventType,
                eventTime = DateTimeOffset.UtcNow,
                run = new { runId },
                job,
                inputs,
                outputs,
                producer = Producer,
                schemaURL = SchemaUrl,
            }));
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            try { diagnostic?.Invoke(exception); }
            catch (Exception diagnosticException) when (diagnosticException is not OutOfMemoryException) { }
        }
    }

    private sealed class RunObservation : IExpressionObservation
    {
        private readonly OpenLineageObserver observer;
        private readonly Guid runId = Guid.NewGuid();
        private int ended;

        public RunObservation(OpenLineageObserver observer)
        {
            this.observer = observer;
            observer.Emit(runId, "START");
        }

        public void Complete() => End("COMPLETE");

        public void Fail(Exception exception) => End("FAIL");

        public void Dispose() => End("FAIL");

        private void End(string eventType)
        {
            if (Interlocked.Exchange(ref ended, 1) == 0)
                observer.Emit(runId, eventType);
        }
    }
}
