namespace Expressif.Benchmark;

internal sealed record VersionUnderTest(string Name, string Directory, AdapterKind AdapterKind);

internal enum AdapterKind
{
    V1,
    V2,
}

internal static class VersionDiscovery
{
    internal const string VersionEnvironmentVariable = "EXPRESSIF_BENCHMARK_VERSION";

    public static IReadOnlyList<VersionUnderTest> Discover()
    {
        var root = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "bin", "versions"));

        if (!Directory.Exists(root))
            throw new DirectoryNotFoundException(
                $"Version directory not found: '{root}'. See benchmark/README.md for setup instructions.");

        var versions = Directory.EnumerateDirectories(root)
            .Select(CreateVersion)
            .OrderBy(version => version.AdapterKind)
            .ThenBy(version => version.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (versions.Length == 0)
            throw new InvalidOperationException($"No version folders were found under '{root}'.");
        if (!versions.Any(version => version.AdapterKind == AdapterKind.V1))
            throw new InvalidOperationException("At least one v1... version folder is required as the ratio baseline.");

        return versions;
    }

    private static VersionUnderTest CreateVersion(string directory)
    {
        var name = Path.GetFileName(directory);
        var adapterKind = name.StartsWith("v1", StringComparison.OrdinalIgnoreCase)
            ? AdapterKind.V1
            : name.StartsWith("v2", StringComparison.OrdinalIgnoreCase)
                ? AdapterKind.V2
                : throw new InvalidOperationException(
                    $"Unsupported version folder '{name}'; folder names must start with v1 or v2.");

        var assemblyPath = Path.Combine(directory, "Expressif.dll");
        if (!File.Exists(assemblyPath))
            throw new FileNotFoundException($"Expressif.dll was not found in '{directory}'.", assemblyPath);

        return new VersionUnderTest(name, Path.GetFullPath(directory), adapterKind);
    }
}
