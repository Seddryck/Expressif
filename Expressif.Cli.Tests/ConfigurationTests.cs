using Expressif.Cli.Application;
using Expressif.Cli.Commands;
using Expressif.Cli.Configuration;
using Expressif.Values;

namespace Expressif.Cli.Tests;

[NonParallelizable]
public class ConfigurationTests
{
    private string directory = null!;
    private CliConfiguration configuration = null!;

    [SetUp]
    public void SetUp()
    {
        directory = Path.Combine(Path.GetTempPath(), "expressif-config-" + Guid.NewGuid().ToString("N"));
        configuration = new CliConfiguration(Path.Combine(directory, CliConfiguration.FileName));
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(directory))
            Directory.Delete(directory, recursive: true);
    }

    [TestCase("output-style", "compact")]
    [TestCase("indent", "2")]
    [TestCase("repl.output-style", "compact")]
    [TestCase("run.indent", "2")]
    public async Task Get_MissingFile_ReturnsDefaultWithoutCreatingFile(string key, string expected)
    {
        var result = await Invoke("config", "get", key);
        Assert.Multiple(() =>
        {
            Assert.That(result.Code, Is.Zero);
            Assert.That(result.Output.Trim(), Is.EqualTo(expected));
            Assert.That(Directory.Exists(directory), Is.False);
        });
    }

    [Test]
    public async Task Set_MissingDirectory_CreatesFileAndPersistsValue()
    {
        var result = await Invoke("config", "set", "repl.output-style", "pretty");
        Assert.That(result.Code, Is.Zero);
        Assert.That(new CliConfiguration(configuration.Path).Get("repl.output-style"), Is.EqualTo("pretty"));
        Assert.That(configuration.Get("run.output-style"), Is.EqualTo("compact"));
    }

    [Test]
    public void Settings_ResolveIndependentlyAndUnsetRestoresInheritance()
    {
        configuration.Set("output-style", "pretty");
        configuration.Set("indent", "4");
        configuration.Set("run.output-style", "compact");
        configuration.Set("repl.indent", "tab");
        Assert.Multiple(() =>
        {
            Assert.That(configuration.Get("run.indent"), Is.EqualTo("4"));
            Assert.That(configuration.Get("repl.output-style"), Is.EqualTo("pretty"));
            Assert.That(configuration.Get("repl.indent"), Is.EqualTo("tab"));
            Assert.That(configuration.Get("evaluate.output-style"), Is.EqualTo("pretty"));
        });
        configuration.Unset("run.output-style");
        Assert.That(configuration.Get("run.output-style"), Is.EqualTo("pretty"));
    }

    [TestCase("unknown", "pretty")]
    [TestCase("repl.unknown", "2")]
    [TestCase("output-style", "wide")]
    [TestCase("indent", "9")]
    [TestCase("indent", "-1")]
    public async Task Set_InvalidValue_DoesNotCreateFile(string key, string value)
    {
        var result = await Invoke("config", "set", key, value);
        Assert.That(result.Code, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
        Assert.That(File.Exists(configuration.Path), Is.False);
    }

    [TestCase("{broken")]
    [TestCase("[]")]
    [TestCase("{\"indent\":9}")]
    [TestCase("{\"repl\":false}")]
    [TestCase("{\"indent\":2,\"indent\":4}")]
    public async Task InvalidJson_IsReportedAndPreserved(string content)
    {
        Directory.CreateDirectory(directory);
        File.WriteAllText(configuration.Path, content);
        var result = await Invoke("config", "set", "indent", "4");
        Assert.That(result.Code, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
        Assert.That(result.Error, Does.Contain("Configuration error"));
        Assert.That(File.ReadAllText(configuration.Path), Is.EqualTo(content));
    }

    [Test]
    public void Set_PreservesUnrelatedSettingsAndLeavesNoTemporaryFiles()
    {
        Directory.CreateDirectory(directory);
        File.WriteAllText(configuration.Path, "{\"custom\":{\"keep\":true},\"run\":{\"custom\":42}}");
        configuration.Set("run.indent", "4");
        var json = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(configuration.Path))!;
        Assert.That(json["custom"]!["keep"]!.GetValue<bool>(), Is.True);
        Assert.That(json["run"]!["custom"]!.GetValue<int>(), Is.EqualTo(42));
        Assert.That(Directory.GetFiles(directory), Has.Length.EqualTo(1));
    }

    [TestCase("evaluate")]
    [TestCase("run")]
    public async Task Commands_UseConfigAndExplicitOptionsOverrideIt(string command)
    {
        configuration.Set("output-style", "compact");
        configuration.Set(command + ".output-style", "pretty");
        configuration.Set("indent", "4");
        var args = command == "evaluate" ? new[] { command, "{1, 2}" } : new[] { command, "reverse", "--input", "{2, 1}" };
        var inherited = await Invoke(args);
        var overridden = await Invoke([.. args, "--compact"]);
        var explicitIndent = await Invoke([.. args, "--indent", "0"]);
        Assert.Multiple(() =>
        {
            Assert.That(inherited.Code, Is.Zero);
            Assert.That(inherited.Output.Trim(), Is.EqualTo("{\n    1,\n    2\n}"));
            Assert.That(overridden.Code, Is.Zero);
            Assert.That(overridden.Output.Trim(), Is.EqualTo("{1, 2}"));
            Assert.That(explicitIndent.Code, Is.Zero);
            Assert.That(explicitIndent.Output.Trim(), Is.EqualTo("{\n1,\n2\n}"));
        });
    }

    [Test]
    public void StoredIndent_AllowedForCompact_ExplicitIndentRejected()
    {
        configuration.Set("indent", "4");
        Assert.That(ConfiguredOutput.TryResolve(configuration, "run", null, false, false, null,
            out var style, out _, out _), Is.True);
        Assert.That(style, Is.EqualTo(ValueFormat.Compact));
        Assert.That(ConfiguredOutput.TryResolve(configuration, "run", null, false, false, "4",
            out _, out _, out _), Is.False);
    }

    [Test]
    public async Task Repl_UsesConfiguredFormatting()
    {
        configuration.Set("repl.output-style", "pretty");
        configuration.Set("repl.indent", "tab");
        var terminal = new TestTerminal();
        var composition = CliComposition.CreateDefault(configuration) with
        {
            Repl = () => new ReplHost(new ReplSession(new Expressif.Cli.Expressions.ExpressionService()), terminal),
        };
        var result = await CliInvoker.InvokeAsync(CliRootCommandFactory.Create(composition, configuration).Parse(["repl"]));
        Assert.That(result, Is.Zero);
        Assert.That(terminal.Result, Is.EqualTo("{\n\t1,\n\t2\n}"));
    }

    [Test]
    public void PackagedDefaults_MatchMissingFileDefaults()
    {
        var packaged = CliConfiguration.CreateDefault();
        Assert.That(File.Exists(packaged.Path), Is.True);
        foreach (var key in new[] { "output-style", "indent", "repl.output-style", "run.indent", "evaluate.indent" })
            Assert.That(packaged.Get(key), Is.EqualTo(configuration.Get(key)));
    }

    [Test]
    public async Task UnsetAndList_ReportInheritedValuesAndSources()
    {
        configuration.Set("indent", "4");
        configuration.Set("repl.indent", "tab");
        var removed = await Invoke("config", "unset", "repl.indent");
        var listed = await Invoke("config", "list", "--command", "repl");
        var path = await Invoke("config", "path");
        Assert.Multiple(() =>
        {
            Assert.That(removed.Code, Is.Zero);
            Assert.That(listed.Code, Is.Zero);
            Assert.That(listed.Output, Does.Contain("repl.indent=4 (source: indent)"));
            Assert.That(listed.Output, Does.Contain("repl.output-style=compact (source: built-in default)"));
            Assert.That(path.Output.Trim(), Is.EqualTo(configuration.Path));
        });
    }

    [Test]
    public async Task InvalidUpdate_PreservesExistingFile()
    {
        configuration.Set("indent", "4");
        var before = File.ReadAllBytes(configuration.Path);
        var result = await Invoke("config", "set", "indent", "invalid");
        Assert.That(result.Code, Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
        Assert.That(File.ReadAllBytes(configuration.Path), Is.EqualTo(before));
    }

    [TestCase("", "compact", "2")]
    [TestCase(" \r\n\t", "compact", "2")]
    [TestCase("{}", "compact", "2")]
    [TestCase("{\"output-style\":\"pretty\"}", "pretty", "2")]
    [TestCase("{\"indent\":4}", "compact", "4")]
    [TestCase("{\"output-style\":null,\"indent\":0}", "compact", "0")]
    [TestCase("{\"output-style\":\"pretty\",\"indent\":null}", "pretty", "2")]
    [TestCase("{\"output-style\":\"\",\"indent\":\"tab\"}", "compact", "tab")]
    [TestCase("{\"output-style\":\"pretty\",\"indent\":\" \"}", "pretty", "2")]
    [TestCase("{\"output-style\":\" \" ,\"indent\":\"\"}", "compact", "2")]
    public void PartialOrEmptyConfiguration_ResolvesEachDefaultIndependently(string content, string style, string indent)
    {
        Directory.CreateDirectory(directory);
        File.WriteAllText(configuration.Path, content);
        foreach (var command in CliConfiguration.Commands)
        {
            Assert.That(configuration.Get(command + ".output-style"), Is.EqualTo(style));
            Assert.That(configuration.Get(command + ".indent"), Is.EqualTo(indent));
        }

        Assert.That(File.ReadAllText(configuration.Path), Is.EqualTo(content));
    }

    [TestCase("null")]
    [TestCase("\"\"")]
    [TestCase("\" \"")]
    public void EmptyCommandOverrides_InheritSharedValuesAndReportSources(string empty)
    {
        Directory.CreateDirectory(directory);
        File.WriteAllText(configuration.Path,
            "{\"output-style\":\"pretty\",\"indent\":0,\"repl\":{\"output-style\":" + empty + ",\"indent\":" + empty + "}}");
        Assert.Multiple(() =>
        {
            Assert.That(configuration.Get("repl.output-style"), Is.EqualTo("pretty"));
            Assert.That(configuration.Get("repl.indent"), Is.EqualTo("0"));
            Assert.That(configuration.GetSource("repl.output-style"), Is.EqualTo("output-style"));
            Assert.That(configuration.GetSource("repl.indent"), Is.EqualTo("indent"));
        });
    }

    [TestCase("evaluate", "0", "")]
    [TestCase("evaluate", "tab", "\t")]
    [TestCase("run", "0", "")]
    [TestCase("run", "tab", "\t")]
    public async Task Commands_UseConfiguredIndentation(string command, string indent, string spaces)
    {
        configuration.Set("output-style", "pretty");
        configuration.Set("indent", indent);
        var args = command == "evaluate" ? new[] { command, "{1, 2}" } : new[] { command, "reverse", "--input", "{2, 1}" };
        var result = await Invoke(args);
        Assert.That(result.Code, Is.Zero);
        Assert.That(result.Output.Trim(), Is.EqualTo($"{{\n{spaces}1,\n{spaces}2\n}}"));
    }

    [TestCase("compact", "--pretty", null, "{\n    1,\n    2\n}")]
    [TestCase("pretty", "--compact", null, "{1, 2}")]
    [TestCase("pretty", "--output-style", "compact", "{1, 2}")]
    [TestCase("pretty", "--style-output", "compact", "{1, 2}")]
    [TestCase("compact", "--style-output", "pretty", "{\n    1,\n    2\n}")]
    [TestCase("pretty", "--indent", "0", "{\n1,\n2\n}")]
    public async Task Repl_ExplicitOptionsOverrideConfiguration(string storedStyle, string option, string? value, string expected)
    {
        configuration.Set("repl.output-style", storedStyle);
        configuration.Set("indent", "4");
        var terminal = new TestTerminal();
        var composition = CliComposition.CreateDefault(configuration) with
        {
            Repl = () => new ReplHost(new ReplSession(new Expressif.Cli.Expressions.ExpressionService()), terminal),
        };
        string[] args = value is null ? ["repl", option] : ["repl", option, value];
        var result = await CliInvoker.InvokeAsync(CliRootCommandFactory.Create(composition, configuration).Parse(args));
        Assert.That(result, Is.Zero);
        Assert.That(terminal.Result, Is.EqualTo(expected));
    }

    [Test]
    public async Task Set_EmptyFile_CreatesUsableConfiguration()
    {
        Directory.CreateDirectory(directory);
        File.WriteAllText(configuration.Path, string.Empty);
        var result = await Invoke("config", "set", "indent", "0");
        Assert.That(result.Code, Is.Zero);
        Assert.That(configuration.Get("indent"), Is.EqualTo("0"));
        Assert.That(configuration.Get("output-style"), Is.EqualTo("compact"));
    }

    private async Task<(int Code, string Output, string Error)> Invoke(params string[] args)
    {
        var previousOut = Console.Out;
        var previousError = Console.Error;
        using var output = new StringWriter();
        using var error = new StringWriter();
        try
        {
            Console.SetOut(output);
            Console.SetError(error);
            var code = await CliInvoker.InvokeAsync(CliRootCommandFactory.Create(configuration: configuration).Parse(args));
            return (code, output.ToString().Replace("\r\n", "\n"), error.ToString());
        }
        finally
        {
            Console.SetOut(previousOut);
            Console.SetError(previousError);
        }
    }

    private sealed class TestTerminal : IReplTerminal
    {
        private bool read;
        public string? Result { get; private set; }
        public string? ReadLine(string prompt, CancellationToken cancellationToken)
        {
            if (read)
                return null;
            read = true;
            return "{1, 2}";
        }

        public void WriteResult(string value) => Result = value;
        public void WriteError(string message) => Assert.Fail(message);
    }
}
