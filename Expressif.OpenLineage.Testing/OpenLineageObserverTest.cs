using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using Expressif.Observability;
using Json.Schema;

namespace Expressif.OpenLineage.Testing;

public class OpenLineageObserverTest
{
    // Vendored from https://openlineage.io/spec/2-0-2/OpenLineage.json.
    private static readonly Lazy<JsonSchema> Schema = new(() => JsonSchema.FromText(
        File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "OpenLineage.schema.json"))));

    [TestCase(true)]
    [TestCase(false)]
    public void Events_ConformToOfficialOpenLineageSchema(bool successful)
    {
        var schema = Schema.Value;
        var transport = new RecordingTransport();
        var observer = new OpenLineageObserver(new OpenLineageOptions
        {
            Expression = "upper",
            Inputs = [OpenLineageDataset.FromFile("customers.json")],
            Outputs = [OpenLineageDataset.FromFile("customers.ndjson")],
        }, transport);
        using (var observation = observer.Begin(ExpressionObservationStage.Evaluate))
        {
            if (successful)
                observation.Complete();
            else
                observation.Fail(new InvalidOperationException());
        }
        foreach (var json in transport.Events)
        {
            using var document = JsonDocument.Parse(json);
            var result = schema.Evaluate(document.RootElement, new EvaluationOptions { RequireFormatValidation = true });
            Assert.That(result.IsValid, Is.True, json);
        }
    }

    [Test]
    public void SuccessfulEvaluation_EmitsLifecycleAndMetadata()
    {
        var transport = new RecordingTransport();
        var input = OpenLineageDataset.FromFile("customers #1.json");
        var output = OpenLineageDataset.FromFile("customers.ndjson");
        var observer = new OpenLineageObserver(new OpenLineageOptions
        {
            Expression = "upper", Namespace = "tests", JobName = "customers", Inputs = [input], Outputs = [output],
        }, transport);
        var expression = new ExpressionFactory(new Expressif.Bindings.ExpressionBinder(), observer: observer).Create("upper");

        Assert.That(expression.Evaluate("alice"), Is.EqualTo("ALICE"));
        var events = transport.Events.ToArray();
        Assert.That(events.Select(EventType), Is.EqualTo(new[] { "START", "COMPLETE" }));
        using var start = JsonDocument.Parse(events[0]);
        using var end = JsonDocument.Parse(events[1]);
        var root = start.RootElement;
        var facet = root.GetProperty("job").GetProperty("facets").GetProperty("expressif_expression");
        Assert.Multiple(() =>
        {
            Assert.That(RunId(root), Is.EqualTo(RunId(end.RootElement)));
            Assert.That(Guid.TryParse(RunId(root), out _), Is.True);
            Assert.That(root.GetProperty("eventTime").GetDateTimeOffset(), Is.LessThanOrEqualTo(end.RootElement.GetProperty("eventTime").GetDateTimeOffset()));
            Assert.That(root.GetProperty("schemaURL").GetString(), Is.EqualTo(OpenLineageObserver.SchemaUrl));
            Assert.That(root.GetProperty("job").GetProperty("namespace").GetString(), Is.EqualTo("tests"));
            Assert.That(root.GetProperty("job").GetProperty("name").GetString(), Is.EqualTo("customers"));
            Assert.That(root.GetProperty("inputs")[0].GetProperty("name").GetString(), Is.EqualTo(input.Name));
            Assert.That(root.GetProperty("outputs")[0].GetProperty("name").GetString(), Is.EqualTo(output.Name));
            Assert.That(facet.GetProperty("expression").GetString(), Is.EqualTo("upper"));
            Assert.That(facet.GetProperty("expressifVersion").GetString(), Is.Not.Empty);
            Assert.That(facet.GetProperty("expressionHash").GetString(), Has.Length.EqualTo(64));
            Assert.That(facet.GetProperty("_schemaURL").GetString(), Is.EqualTo(OpenLineageObserver.FacetSchemaUrl));
            Assert.That(facet.GetProperty("_producer").GetString(), Is.EqualTo(root.GetProperty("producer").GetString()));
        });
    }

    [Test]
    public void FailedEvaluation_EmitsFailAndPreservesException()
    {
        var transport = new RecordingTransport();
        var observer = CreateObserver(transport);
        var expression = new ExpressionFactory(new Expressif.Bindings.ExpressionBinder(), observer: observer).Create("fold(sum)");
        Assert.Catch(() => expression.Evaluate(new[] { "unknown" }));
        Assert.That(transport.Events.Select(EventType), Is.EqualTo(new[] { "START", "FAIL" }));
    }

    [Test]
    public void ConcurrentEvaluations_HaveIndependentRuns()
    {
        var transport = new RecordingTransport();
        var expression = new ExpressionFactory(new Expressif.Bindings.ExpressionBinder(), observer: CreateObserver(transport)).Create("upper");
        Parallel.For(0, 30, _ => expression.Evaluate("alice"));
        var runs = transport.Events.Select(json =>
        {
            using var document = JsonDocument.Parse(json);
            return (Id: RunId(document.RootElement), Type: EventType(json));
        }).GroupBy(item => item.Id).ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(runs, Has.Length.EqualTo(30));
            Assert.That(runs, Has.All.Matches<IGrouping<string?, (string? Id, string? Type)>>(run => run.Select(item => item.Type).SequenceEqual(["START", "COMPLETE"])));
        });
    }

    [Test]
    public void Scope_EmitsOnlyOneTerminalEventAndFailsWhenAbandoned()
    {
        var transport = new RecordingTransport();
        var observer = CreateObserver(transport);
        using (var scope = observer.Begin(ExpressionObservationStage.Evaluate))
        {
            scope.Complete();
            scope.Fail(new Exception());
            scope.Complete();
        }
        observer.Begin(ExpressionObservationStage.Evaluate).Dispose();
        Assert.That(transport.Events.Select(EventType), Is.EqualTo(new[] { "START", "COMPLETE", "START", "FAIL" }));
    }

    [Test]
    public void TransportAndDiagnosticFailures_DoNotChangeEvaluation()
    {
        var diagnostics = 0;
        var observer = new OpenLineageObserver(new OpenLineageOptions { Expression = "upper" }, new ThrowingTransport(), _ =>
        {
            Interlocked.Increment(ref diagnostics);
            throw new InvalidOperationException("Diagnostic failed");
        });
        var expression = new ExpressionFactory(new Expressif.Bindings.ExpressionBinder(), observer: observer).Create("upper");
        Assert.Multiple(() =>
        {
            Assert.That(expression.Evaluate("alice"), Is.EqualTo("ALICE"));
            Assert.That(diagnostics, Is.EqualTo(2));
        });
    }

    [Test]
    public void FileDataset_NormalizesRelativePathsAndEscapesCharacters()
    {
        var dataset = OpenLineageDataset.FromFile("folder/../customers #1.json");
        var uri = new Uri(Path.GetFullPath("customers #1.json"));
        Assert.Multiple(() =>
        {
            Assert.That(dataset.Namespace + dataset.Name, Is.EqualTo(uri.AbsoluteUri));
            Assert.That(dataset.Name, Does.Contain("%20%231.json"));
        });
    }

    [Test]
    public void HttpTransport_PostsJsonToConfiguredEndpointWithBearerToken()
    {
        using var handler = new RecordingHttpHandler();
        using var client = new HttpClient(handler);
        var transport = new HttpOpenLineageTransport(client, new Uri("http://localhost:5000/prefix"), "custom/events", "test-token");
        transport.Emit("{\"eventType\":\"START\"}");
        Assert.Multiple(() =>
        {
            Assert.That(handler.Url, Is.EqualTo("http://localhost:5000/prefix/custom/events"));
            Assert.That(handler.Method, Is.EqualTo(HttpMethod.Post));
            Assert.That(handler.ContentType, Is.EqualTo("application/json"));
            Assert.That(handler.Authorization, Is.EqualTo("Bearer test-token"));
            Assert.That(handler.Body, Is.EqualTo("{\"eventType\":\"START\"}"));
        });
    }

    [Test]
    public void HttpTransport_UnsuccessfulResponseIsReported()
    {
        using var handler = new RecordingHttpHandler { StatusCode = HttpStatusCode.InternalServerError };
        using var client = new HttpClient(handler);
        var transport = new HttpOpenLineageTransport(client, new Uri("http://localhost:5000"));
        Assert.Throws<HttpRequestException>(() => transport.Emit("{}"));
    }

    private static OpenLineageObserver CreateObserver(IOpenLineageTransport transport)
        => new(new OpenLineageOptions { Expression = "upper" }, transport);

    private static string? EventType(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty("eventType").GetString();
    }

    private static string? RunId(JsonElement root) => root.GetProperty("run").GetProperty("runId").GetString();

    private sealed class RecordingTransport : IOpenLineageTransport
    {
        public ConcurrentQueue<string> Events { get; } = new();

        public void Emit(string json) => Events.Enqueue(json);
    }

    private sealed class ThrowingTransport : IOpenLineageTransport
    {
        public void Emit(string json) => throw new HttpRequestException("Backend unavailable");
    }

    private sealed class RecordingHttpHandler : HttpMessageHandler
    {
        public HttpStatusCode StatusCode { get; init; } = HttpStatusCode.OK;
        public string? Url { get; private set; }
        public HttpMethod? Method { get; private set; }
        public string? ContentType { get; private set; }
        public string? Authorization { get; private set; }
        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Url = request.RequestUri?.AbsoluteUri;
            Method = request.Method;
            ContentType = request.Content?.Headers.ContentType?.MediaType;
            Authorization = request.Headers.Authorization?.ToString();
            Body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(StatusCode);
        }
    }
}
