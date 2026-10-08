# Expressif.OpenLineage

Optional OpenLineage reporting through `Expressif.Observability.IExpressionObserver`.
The integration uses OpenLineage 2.0.2 run events and works with compatible HTTP
backends, including Marquez. The core Expressif project has no dependency on this
package or on OpenLineage types.

```csharp
using Expressif;
using Expressif.Bindings;
using Expressif.OpenLineage;

using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
var transport = new HttpOpenLineageTransport(client, new Uri("http://localhost:5000"));
var observer = new OpenLineageObserver(new OpenLineageOptions
{
    Namespace = "my-application",
    JobName = "customers",
    Expression = "upper",
    FunctionMetrics = true,
    FlowDecisions = true,
    Inputs = [OpenLineageDataset.FromFile("customers.json")],
    Outputs = [OpenLineageDataset.FromFile("customers.ndjson")],
}, transport, exception => Console.Error.WriteLine(exception.Message));
var expression = new ExpressionFactory(new ExpressionBinder(), observer: observer).Create("upper");
var value = expression.Evaluate("alice");
```

Set `FunctionMetrics` to `true` to include aggregated null/non-null function metrics on terminal run
events. The evaluation observation exposes the function capability automatically:

```csharp
var expression = new ExpressionFactory(new ExpressionBinder(), observer: observer)
    .Create("trim | upper");
```

The `expressif_functionMetrics` run facet distinguishes bound nodes by stable ID and contains no raw
values. Each entry counts invocations, null/non-null inputs and outputs, and errors. Collections count
as a single non-null boundary value and are not enumerated by observation.

Set `FlowDecisions` to `true` to add the `expressif_flowDecisions` run facet. It aggregates semantic
outcomes for `catch`, conditional flow, `coalesce`, `switch`, `try`, `guard`, and `throw` by stable
bound-node ID without retaining raw values.

Each evaluation emits `START` and then `COMPLETE` or `FAIL` with the same unique
run ID. Parse and bind scopes do not generate separate jobs. The custom
`expressif_expression` job facet includes the original expression, Expressif
assembly version, and SHA-256 hash of the expression's exact UTF-8 text. It uses
the supported specification's extensible BaseFacet schema.

Provide datasets only when their identities are known. File datasets use the
file URI authority as namespace and its absolute escaped path as name. No
datasets are inferred from values or streams. Options and dataset lists are
snapshotted when the observer is constructed. Transports shared across
evaluations must be safe for concurrent use; the HTTP transport is.

For a larger execution that includes lazy enumeration, serialization, or writing
an output, create and activate an observation with
`observer.Create(ExpressionObservationStage.Evaluate)` and `observation.Activate()` around that work.
Call `Complete()` only once all work succeeds, or
`Fail(exception)` on failure. Disposing an unfinished scope emits `FAIL`.
An `ExpressionFactory` evaluation scope ends when `Evaluate` returns; it does not
track later consumption of a lazy result. The CLI scopes cover the full command,
including input enumeration and output serialization for `run` and `evaluate`.

HTTP delivery is synchronous. It posts JSON to `api/v1/lineage` appended to the
base URL, with an optional relative endpoint override and Bearer API key. The
caller owns the HTTP client and chooses its timeout. Delivery failures invoke
the optional diagnostic callback without changing the expression result or
exception. Delivery is best effort; failed requests are not retried.

See [CLI configuration](../docs/cli/configuration.md) for environment settings.
