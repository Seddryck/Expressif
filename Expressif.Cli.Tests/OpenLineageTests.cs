using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Expressif.Cli.Application;

namespace Expressif.Cli.Tests;

[NonParallelizable]
public class OpenLineageTests
{
    private static readonly string[] Variables =
    ["OPENLINEAGE_URL", "OPENLINEAGE_ENDPOINT", "OPENLINEAGE_API_KEY", "OPENLINEAGE_NAMESPACE", "OPENLINEAGE_JOB_NAME", "OPENLINEAGE_DISABLED"];
    private readonly List<string> files = [];
    private string?[] saved = [];

    [SetUp]
    public void SetUp()
    {
        saved = Variables.Select(Environment.GetEnvironmentVariable).ToArray();
        foreach (var name in Variables)
            Environment.SetEnvironmentVariable(name, null);
    }

    [TearDown]
    public void TearDown()
    {
        for (var i = 0; i < Variables.Length; i++)
            Environment.SetEnvironmentVariable(Variables[i], saved[i]);
        foreach (var file in files)
            File.Delete(file);
        files.Clear();
    }

    [TestCase("run", "upper", "--input", "\"alice\"")]
    [TestCase("evaluate", "upper", "--input", "\"alice\"")]
    public async Task Enabled_ReportsLifecycleWithoutContaminatingStdout(string command, string expression, string option, string input)
    {
        using var backend = new Backend();
        Environment.SetEnvironmentVariable("OPENLINEAGE_URL", backend.Url);
        Environment.SetEnvironmentVariable("OPENLINEAGE_NAMESPACE", "cli-tests");
        Environment.SetEnvironmentVariable("OPENLINEAGE_JOB_NAME", "customer-job");
        Environment.SetEnvironmentVariable("OPENLINEAGE_API_KEY", "test-token");
        var receiving = backend.ReceiveAsync(2);
        var result = await InvokeAsync(command, expression, option, input);
        var events = await receiving;
        Assert.Multiple(() =>
        {
            Assert.That(result.Code, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.Output.Trim(), Is.EqualTo("ALICE"));
            Assert.That(result.Error, Is.Empty);
            Assert.That(events.Select(EventType), Is.EqualTo(new[] { "START", "COMPLETE" }));
            Assert.That(backend.Paths, Is.EqualTo(new[] { "/api/v1/lineage", "/api/v1/lineage" }));
            Assert.That(backend.Authorization, Has.All.EqualTo("Bearer test-token"));
        });
        using var start = JsonDocument.Parse(events[0]);
        using var end = JsonDocument.Parse(events[1]);
        Assert.Multiple(() =>
        {
            Assert.That(start.RootElement.GetProperty("run").GetProperty("runId").GetString(), Is.EqualTo(end.RootElement.GetProperty("run").GetProperty("runId").GetString()));
            Assert.That(start.RootElement.GetProperty("job").GetProperty("namespace").GetString(), Is.EqualTo("cli-tests"));
            Assert.That(start.RootElement.GetProperty("job").GetProperty("name").GetString(), Is.EqualTo("customer-job"));
            Assert.That(start.RootElement.GetProperty("inputs").GetArrayLength(), Is.Zero);
            Assert.That(start.RootElement.GetProperty("outputs").GetArrayLength(), Is.Zero);
        });
    }

    [TestCase("run")]
    [TestCase("evaluate")]
    public async Task FailedExecution_ReportsFailAndPreservesExitCode(string command)
    {
        using var backend = new Backend();
        Environment.SetEnvironmentVariable("OPENLINEAGE_URL", backend.Url);
        var receiving = backend.ReceiveAsync(2);
        var result = await InvokeAsync(command, "fold(sum)", "--input", "{\"unknown\"}");
        var events = await receiving;
        Assert.Multiple(() =>
        {
            Assert.That(result.Code, Is.EqualTo(ExitCodes.EvaluationFailed));
            Assert.That(result.Output, Is.Empty);
            Assert.That(result.Error, Is.Not.Empty);
            Assert.That(events.Select(EventType), Is.EqualTo(new[] { "START", "FAIL" }));
        });
    }

    [Test]
    public async Task SourceFile_ReportsSingleRunForAllRowsWithKnownDataset()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        File.WriteAllText(path, "[\"alice\",\"bob\"]");
        files.Add(path);
        using var backend = new Backend();
        Environment.SetEnvironmentVariable("OPENLINEAGE_URL", backend.Url);
        Environment.SetEnvironmentVariable("OPENLINEAGE_ENDPOINT", "custom/events");
        var receiving = backend.ReceiveAsync(2);
        var result = await InvokeAsync("run", "upper", "--source", path);
        var events = await receiving;
        using var document = JsonDocument.Parse(events[0]);
        var dataset = document.RootElement.GetProperty("inputs")[0];
        Assert.Multiple(() =>
        {
            Assert.That(result.Code, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.Output.Replace("\r\n", "\n"), Is.EqualTo("ALICE\nBOB\n"));
            Assert.That(events.Select(EventType), Is.EqualTo(new[] { "START", "COMPLETE" }));
            Assert.That(dataset.GetProperty("namespace").GetString() + dataset.GetProperty("name").GetString(), Is.EqualTo(new Uri(path).AbsoluteUri));
            Assert.That(backend.Paths, Has.All.EqualTo("/custom/events"));
        });
    }

    [Test]
    public async Task MissingSource_ReportsFail()
    {
        using var backend = new Backend();
        Environment.SetEnvironmentVariable("OPENLINEAGE_URL", backend.Url);
        var receiving = backend.ReceiveAsync(2);
        var result = await InvokeAsync("run", "upper", "--source", Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json"));
        Assert.Multiple(() =>
        {
            Assert.That(result.Code, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(result.Output, Is.Empty);
        });
        Assert.That((await receiving).Select(EventType), Is.EqualTo(new[] { "START", "FAIL" }));
    }

    [TestCase(null)]
    [TestCase("true")]
    public async Task DisabledOrUnconfigured_PreservesBehaviour(string? disabled)
    {
        if (disabled is not null)
        {
            Environment.SetEnvironmentVariable("OPENLINEAGE_URL", "invalid-url");
            Environment.SetEnvironmentVariable("OPENLINEAGE_DISABLED", disabled);
        }
        var result = await InvokeAsync("run", "upper", "--input", "\"alice\"");
        Assert.Multiple(() =>
        {
            Assert.That(result.Code, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.Output.Trim(), Is.EqualTo("ALICE"));
            Assert.That(result.Error, Is.Empty);
        });
    }

    [Test]
    public async Task InvalidConfiguration_OnlyAddsStderrDiagnostic()
    {
        Environment.SetEnvironmentVariable("OPENLINEAGE_URL", "invalid-url");
        var result = await InvokeAsync("run", "upper", "--input", "\"alice\"");
        Assert.Multiple(() =>
        {
            Assert.That(result.Code, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.Output.Trim(), Is.EqualTo("ALICE"));
            Assert.That(result.Error, Does.Contain("OpenLineage reporting failed:"));
        });
    }

    [Test]
    public async Task BackendFailure_DoesNotFailExecution()
    {
        using var backend = new Backend { StatusCode = 500 };
        Environment.SetEnvironmentVariable("OPENLINEAGE_URL", backend.Url);
        var receiving = backend.ReceiveAsync(2);
        var result = await InvokeAsync("run", "upper", "--input", "\"alice\"");
        await receiving;
        Assert.Multiple(() =>
        {
            Assert.That(result.Code, Is.EqualTo(ExitCodes.Success));
            Assert.That(result.Output.Trim(), Is.EqualTo("ALICE"));
            Assert.That(result.Error, Does.Contain("OpenLineage reporting failed:"));
        });
    }

    [Test]
    public async Task Repl_ReportsEvaluations()
    {
        using var backend = new Backend();
        Environment.SetEnvironmentVariable("OPENLINEAGE_URL", backend.Url);
        var receiving = backend.ReceiveAsync(2);
        var session = new ReplSession(new Expressions.ExpressionService());
        Assert.That(session.Execute("\"alice\" | upper"), Is.TypeOf<ReplEvaluationResult>());
        Assert.That((await receiving).Select(EventType), Is.EqualTo(new[] { "START", "COMPLETE" }));
    }

    private static string? EventType(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty("eventType").GetString();
    }

    private static async Task<(int Code, string Output, string Error)> InvokeAsync(params string[] args)
    {
        var originalOutput = Console.Out;
        var originalError = Console.Error;
        using var output = new StringWriter();
        using var error = new StringWriter();
        Console.SetOut(output);
        Console.SetError(error);
        try
        {
            var code = await CliInvoker.InvokeAsync(args);
            return (code, output.ToString(), error.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
            Console.SetError(originalError);
        }
    }

    private sealed class Backend : IDisposable
    {
        private readonly HttpListener listener = new();

        public Backend()
        {
            using var socket = new TcpListener(IPAddress.Loopback, 0);
            socket.Start();
            var port = ((IPEndPoint)socket.LocalEndpoint).Port;
            socket.Stop();
            Url = $"http://localhost:{port}";
            listener.Prefixes.Add(Url + "/");
            listener.Start();
        }

        public string Url { get; }
        public int StatusCode { get; init; } = 200;
        public List<string?> Paths { get; } = [];
        public List<string?> Authorization { get; } = [];

        public async Task<string[]> ReceiveAsync(int count)
        {
            var events = new List<string>();
            for (var i = 0; i < count; i++)
            {
                var context = await listener.GetContextAsync().WaitAsync(TimeSpan.FromSeconds(15));
                using var reader = new StreamReader(context.Request.InputStream);
                events.Add(await reader.ReadToEndAsync());
                Paths.Add(context.Request.Url?.AbsolutePath);
                Authorization.Add(context.Request.Headers["Authorization"]);
                context.Response.StatusCode = StatusCode;
                context.Response.Close();
            }
            return events.ToArray();
        }

        public void Dispose() => listener.Close();
    }
}
