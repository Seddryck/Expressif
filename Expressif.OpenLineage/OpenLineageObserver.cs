using System.Collections.Concurrent;
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
public sealed class OpenLineageObserver : IExpressionObserver, IFunctionObserver
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
    private readonly AsyncLocal<RunObservation?> current = new();

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
            ? new RunObservation(this, current.Value)
            : IgnoredObservation.Instance;

    public void OnCompleted(FunctionObservationContext context, object? input, object? output)
        => current.Value?.Complete(context, input, output);

    public void OnFailed(FunctionObservationContext context, object? input, Exception exception)
        => current.Value?.Fail(context, input);

    private static object[] Datasets(IReadOnlyList<OpenLineageDataset> datasets)
        => datasets.Select(dataset =>
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(dataset.Namespace);
            ArgumentException.ThrowIfNullOrWhiteSpace(dataset.Name);
            return (object)new { @namespace = dataset.Namespace, name = dataset.Name };
        }).ToArray();

    private void Emit(Guid runId, string eventType, object? functionMetrics = null)
    {
        try
        {
            transport.Emit(JsonSerializer.Serialize(new
            {
                eventType,
                eventTime = DateTimeOffset.UtcNow,
                run = functionMetrics is null
                    ? (object)new { runId }
                    : new { runId, facets = new { expressif_functionMetrics = functionMetrics } },
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
        private readonly RunObservation? previous;
        private readonly Guid runId = Guid.NewGuid();
        private readonly ConcurrentDictionary<string, FunctionMetric> metrics = new(StringComparer.Ordinal);
        private int ended;

        public RunObservation(OpenLineageObserver observer, RunObservation? previous)
        {
            this.observer = observer;
            this.previous = previous;
            observer.current.Value = this;
            observer.Emit(runId, "START");
        }

        public void Complete() => End("COMPLETE");

        public void Fail(Exception exception) => End("FAIL");

        public void Dispose()
        {
            End("FAIL");
            observer.current.Value = previous;
        }

        public void Complete(FunctionObservationContext context, object? input, object? output)
            => metrics.GetOrAdd(context.Id, _ => new(context)).Complete(input, output);

        public void Fail(FunctionObservationContext context, object? input)
            => metrics.GetOrAdd(context.Id, _ => new(context)).Fail(input);

        private void End(string eventType)
        {
            if (Interlocked.Exchange(ref ended, 1) == 0)
                observer.Emit(runId, eventType, CreateFacet());
        }

        private object? CreateFacet()
        {
            if (metrics.IsEmpty)
                return null;
            return new
            {
                _producer = Producer,
                _schemaURL = FacetSchemaUrl,
                functions = metrics.Values
                    .OrderBy(metric => metric.Sequence)
                    .Select(metric => metric.Snapshot())
                    .ToArray(),
            };
        }
    }

    private sealed class FunctionMetric(FunctionObservationContext context)
    {
        private long invocationCount;
        private long inputNullCount;
        private long inputNonNullCount;
        private long outputNullCount;
        private long outputNonNullCount;
        private long errorCount;

        public FunctionObservationContext Context { get; } = context;
        public long Sequence { get; } = long.Parse(
            context.Id.AsSpan("function[".Length, context.Id.Length - "function[]".Length),
            System.Globalization.CultureInfo.InvariantCulture);

        public void Complete(object? input, object? output)
        {
            CountInput(input);
            if (IsNull(output))
                Interlocked.Increment(ref outputNullCount);
            else
                Interlocked.Increment(ref outputNonNullCount);
            Interlocked.Increment(ref invocationCount);
        }

        public void Fail(object? input)
        {
            CountInput(input);
            Interlocked.Increment(ref errorCount);
            Interlocked.Increment(ref invocationCount);
        }

        private void CountInput(object? input)
        {
            if (IsNull(input))
                Interlocked.Increment(ref inputNullCount);
            else
                Interlocked.Increment(ref inputNonNullCount);
        }

        public object Snapshot() => new
        {
            id = Context.Id,
            name = Context.Name,
            invocationCount = Interlocked.Read(ref invocationCount),
            input = new
            {
                nullCount = Interlocked.Read(ref inputNullCount),
                nonNullCount = Interlocked.Read(ref inputNonNullCount),
            },
            output = new
            {
                nullCount = Interlocked.Read(ref outputNullCount),
                nonNullCount = Interlocked.Read(ref outputNonNullCount),
            },
            errorCount = Interlocked.Read(ref errorCount),
        };

        private static bool IsNull(object? value)
            => Expressif.Values.Special.Null.Instance.Equals(value);
    }

    private sealed class IgnoredObservation : IExpressionObservation
    {
        public static IgnoredObservation Instance { get; } = new();

        private IgnoredObservation() { }

        public void Dispose() { }
    }
}
