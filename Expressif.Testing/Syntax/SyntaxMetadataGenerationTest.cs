using System.Diagnostics;

namespace Expressif.Testing.Syntax;

[TestFixture]
public class SyntaxMetadataGenerationTest
{
    [Test]
    public void GeneratedHighlighters_ContainTypeLiteralsAndMappingOperator()
    {
        var root = FindRepositoryRoot();
        var output = Path.Combine(Path.GetTempPath(), $"expressif-{Guid.NewGuid():N}");
        Directory.CreateDirectory(output);

        try
        {
            var textMate = Path.Combine(output, "expressif.tmLanguage.json");
            var rouge = Path.Combine(output, "expressif.rb");
            RunPowerShell(root, "Expressif.Syntax/New-tmLanguage.ps1", textMate);
            RunPowerShell(root, "Expressif.Syntax/New-RougeLexer.ps1", rouge);

            Assert.Multiple(() =>
            {
                Assert.That(File.ReadAllText(textMate), Does.Contain(":boolean").And.Contain(":integer").And.Contain("->"));
                Assert.That(File.ReadAllText(rouge), Does.Contain(":boolean").And.Contain(":integer").And.Contain("->"));
            });
        }
        finally
        {
            Directory.Delete(output, true);
        }
    }

    private static void RunPowerShell(string root, string script, string output)
    {
        var start = new ProcessStartInfo("pwsh")
        {
            WorkingDirectory = Path.Combine(root, "Expressif.Syntax"),
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };
        start.ArgumentList.Add("-NoProfile");
        start.ArgumentList.Add("-File");
        start.ArgumentList.Add(Path.Combine(root, script));
        start.ArgumentList.Add("-InputFolder");
        start.ArgumentList.Add(Path.Combine(root, "docs", "_data"));
        start.ArgumentList.Add("-OutputPath");
        start.ArgumentList.Add(output);

        using var process = Process.Start(start)!;
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();
        Assert.That(process.ExitCode, Is.Zero, $"{stdout}{Environment.NewLine}{stderr}");
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "Expressif.sln")))
            current = current.Parent;

        return current?.FullName ?? throw new DirectoryNotFoundException("Could not locate the Expressif repository root.");
    }
}
