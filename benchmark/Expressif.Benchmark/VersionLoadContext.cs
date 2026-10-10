using System.Reflection;
using System.Runtime.Loader;

namespace Expressif.Benchmark;

internal sealed class VersionLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver resolver;
    private readonly string versionDirectory;

    public VersionLoadContext(string mainAssemblyPath)
        : base($"Expressif benchmark: {Path.GetFileName(Path.GetDirectoryName(mainAssemblyPath))}", isCollectible: false)
        => (resolver, versionDirectory) = (
            new AssemblyDependencyResolver(mainAssemblyPath),
            Path.GetDirectoryName(mainAssemblyPath)!);

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var path = resolver.ResolveAssemblyToPath(assemblyName)
            ?? FindSuppliedAssembly(assemblyName);
        return path is null ? null : LoadFromAssemblyPath(path);
    }

    private string? FindSuppliedAssembly(AssemblyName assemblyName)
    {
        var path = Path.Combine(versionDirectory, $"{assemblyName.Name}.dll");
        return File.Exists(path) ? path : null;
    }

    protected override nint LoadUnmanagedDll(string unmanagedDllName)
    {
        var path = resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return path is null ? nint.Zero : LoadUnmanagedDllFromPath(path);
    }
}
