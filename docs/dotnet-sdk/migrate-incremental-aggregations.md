---
layout: docs
title: Migrate incremental aggregations to v3
parent: .NET SDK
nav_order: 8
description: Replace the v2 mutable accumulator lifecycle with reusable aggregation definitions and isolated sessions.
---

For application-wide package and composition changes, first read [Migrate from v2 to v3](migrate-v2-to-v3.md). This page covers the additional source changes required by custom incremental aggregations.

In v3, custom incremental aggregation functions implement `IIncrementalAggregation` or inherit from `BaseIncrementalAggregation`. An aggregation definition is reusable. Each call to `CreateSession()` returns fresh mutable state owned by one evaluation.

The v2 `IAccumulator` and `BaseAccumulator` APIs are removed. There is no runtime adapter: an adapter around a shared v2 accumulator could not guarantee state isolation or safe concurrent evaluation. Migrate the implementation by moving its mutable fields and update logic into a session.

## Replace the lifecycle

The v2 shape mixed reusable function configuration with one evaluation's state:

```csharp
public sealed class SumOfSquares : BaseAccumulator
{
    private decimal sum;

    public override void Initialize() => sum = 0;
    public override void Accumulate(object? item) => sum += (decimal)item! * (decimal)item;
    public override object GetValue() => sum;
}
```

The v3 shape separates the definition from each run:

```csharp
using Expressif.Functions.Accumulation;

public sealed class SumOfSquares : BaseIncrementalAggregation
{
    public override IAggregationSession CreateSession() => new Session();

    private sealed class Session : IAggregationSession
    {
        private decimal sum;

        public void Add(object? item)
            => sum += (decimal)item! * (decimal)item;

        public object Snapshot() => sum;
    }
}
```

`CreateSession()` replaces initialization, `Add()` replaces accumulation, and `Snapshot()` projects the result accumulated so far. A newly created session represents empty input. `Snapshot()` is non-terminal: `scan` takes a snapshot after each item and then continues adding. A returned snapshot must remain stable when later items are added.

Store constructor arguments and provider delegates on the aggregation definition. Evaluate providers whose values belong to one run inside `CreateSession()`, then pass those values to the session. Do not store mutable evaluation state on the definition. Sessions belong to one evaluation and do not need to be thread-safe; definitions must support concurrent calls to `CreateSession()` and `Evaluate()`.

Update extension signatures from `Func<IAccumulator>` to `Func<IIncrementalAggregation>`. `fold`, `scan`, `broadcast`, and `summarize-against` create sessions themselves and preserve their existing expression syntax and results.
