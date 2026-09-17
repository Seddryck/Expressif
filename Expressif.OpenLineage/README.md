# Expressif.OpenLineage

Optional OpenLineage reporting through `Expressif.Observability.IExpressionObserver`.
The integration uses OpenLineage 2.0.2 run events and works with compatible HTTP
backends, including Marquez. The core Expressif project has no dependency on this
package or on OpenLineage types.

```csharp
using Expressif;
using Expressif.OpenLineage;

using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
var transport = new HttpOpenLineageTransport(client, new Uri("http://localhost:5000"));
var observer = new OpenLineageObserver(new OpenLineageOptions
{
    Namespace = "my-application",
    JobName = "customers",
    Expression = "upper",
    Inputs = [OpenLineageDataset.FromFile("customers.json")],
    Outputs = [OpenLineageDataset.FromFile("customers.ndjson")],
}, transport, exception => Console.Error.WriteLine(exception.Message));
var expression = new ExpressionFactory(observer: observer).Create("upper");
var value = expression.Evaluate("alice");
```

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
an output, use the same observer's `Begin(ExpressionObservationStage.Evaluate)`
scope around that work and call `Complete()` only once all work succeeds, or
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
