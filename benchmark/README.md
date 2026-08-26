# Expressif benchmarks

This Windows-only .NET 10 harness compares manually supplied Expressif builds. It never
builds, restores, downloads, or checks out an Expressif version.

## Supply versions

Build the versions separately, then copy each complete output (including dependencies) to:

```text
benchmark/bin/versions/
├── v1.8.0/
│   ├── Expressif.dll
│   └── ...dependencies...
└── v2-next-major/
    ├── Expressif.dll
    └── ...dependencies...
```

Folder names beginning with `v1` use the v1 constructor API. Names beginning with `v2`
use the next-major factory API. Every folder is loaded in an isolated
`AssemblyLoadContext`. The first v1 folder is BenchmarkDotNet's baseline; additional v1
and v2 folders become separate jobs.

## Run

From the repository root:

```powershell
dotnet run --project benchmark/Expressif.Benchmark -c Release
```

Pass normal BenchmarkDotNet arguments after `--`, for example `-- --filter *Evaluate*`.
Reflection, assembly loading, expression construction, and delegate creation happen in
global setup for evaluation benchmarks. The construction benchmark measures only the
version's native parse-and-bind call.

## Test the supplied binaries

With version folders populated as above, run:

```powershell
dotnet test benchmark/Expressif.Benchmark.sln -c Release
```

The adapter tests verify the coercion pipeline on each available API generation. They
also verify that the complex v1 workload parses, binds, and evaluates successfully.
Tests are skipped with a setup explanation when their required manually supplied version
is absent.
