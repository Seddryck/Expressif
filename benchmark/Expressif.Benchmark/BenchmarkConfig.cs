using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;

namespace Expressif.Benchmark;

internal static class BenchmarkConfig
{
    public static IConfig Create(IReadOnlyList<VersionUnderTest> versions)
    {
        var config = ManualConfig.Create(DefaultConfig.Instance)
            .AddDiagnoser(MemoryDiagnoser.Default);

        var baselineAssigned = false;
        foreach (var version in versions)
        {
            var job = Job.Default
                .WithId(version.Name)
                .WithEnvironmentVariable(VersionDiscovery.VersionEnvironmentVariable, version.Directory);

            if (!baselineAssigned && version.AdapterKind == AdapterKind.V1)
            {
                job = job.AsBaseline();
                baselineAssigned = true;
            }

            config.AddJob(job);
        }

        return config;
    }
}
