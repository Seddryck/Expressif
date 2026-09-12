using BenchmarkDotNet.Running;
using Expressif.Benchmark;

if (!OperatingSystem.IsWindows())
{
    Console.Error.WriteLine("Expressif benchmarks support Windows only.");
    return 1;
}

try
{
    var versions = VersionDiscovery.Discover();
    BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
        .Run(args, BenchmarkConfig.Create(versions));
    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}
