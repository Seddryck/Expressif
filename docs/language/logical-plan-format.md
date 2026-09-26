# Logical plan JSON format

Expressif logical plans are a language-level interchange format. They contain canonical operator and parameter descriptors, semantic literal types, argument context depth, traversal and evaluation rules, structural semantics, spread markers, and omission contracts. They never contain CLR types, assemblies, delegates, backend implementation objects, or human-readable catalog descriptions.

The top-level object identifies the format as `expressif.logical-plan`, declares its integer `version`, identifies the compatible catalog line with `catalogCompatibility`, and contains a pipeline in `plan`. Version 1 is described by [`logical-plan.schema.json`](../_data/logical-plan.schema.json).

## Compatibility

Readers must reject an unknown format name, plan version, node kind, or catalog compatibility level. A reader may accept added optional fields only in a later format version whose compatibility rules explicitly allow them; version 1 objects reject undeclared fields. Existing fields and node kinds do not change meaning within a format version.

Writers emit canonical operator names and properties in deterministic order. The same normalized plan therefore produces byte-for-byte identical compact JSON. Consumers should use semantic `type` values to interpret literals and must not infer host-language runtime types.

## Nodes

A `pipeline` contains ordered `items`. A `call` contains its canonical operator descriptor, context depth, and arguments in canonical parameter order. Each argument records whether it was explicitly supplied, whether it spreads its value, and either its value or its omission behavior. A `literal` pairs an Expressif semantic type with its JSON representation; temporal and duration values use invariant strings. Special scalar literals retain their canonical source spelling: `all` uses `#all`, while `ordering` uses `#less`, `#equal`, or `#greater`.

References and structured value constructors use calls rather than private node kinds. For example, `.age` uses `field`, `$1` uses `tuple-at`, and array and tuple literals use `array` and `tuple`.

Input-binding expressions use an `input-binding` call. Its `names` argument is an array of declared names, `positional` distinguishes tuple destructuring from named or anonymous binding, and `body` contains the complete bound pipeline. An empty names array with `positional` set to `false` represents an anonymous binding.

## Traversal and evaluation

An operator's optional `traversal` object identifies the value source and the selection visited within it. A parameter's optional `evaluation` object identifies its `frequency` and, when applicable, its `source` or `context`. Evaluation frequency is one of `once`, `per-element`, or `custom`.

These objects intentionally contain only machine-readable fields. The catalog retains the explanatory summaries used by generated reference pages, but portable plans omit that prose so downstream consumers do not need to interpret or preserve human-language text.

## Structural semantics

An operator can include a `semantics` object with all three structural dimensions. The values have the same meaning as in [Structural semantics](structural-semantics.md):

| Dimension | Allowed values |
|:----------|:---------------|
| `cardinality` | `preserved`, `non-increasing`, `collapsed`, `expanded`, `partitioned`, `unknown` |
| `dependency` | `per-element`, `prefix`, `whole-input`, `partition`, `unknown` |
| `ordering` | `preserved`, `reordered`, `unordered`, `not-applicable`, `unknown` |

The object is omitted when the catalog declares no structural relationship for the operator. When present, consumers can use it directly for lineage, schema analysis, optimization, or telemetry decisions without classifying operators by name.

For `{1, 2, 3} | broadcast(sum)`, the `broadcast` operator declares `preserved` cardinality, `whole-input` dependency, and `preserved` ordering. In contrast, `map` has `per-element` dependency, while `fold` has `collapsed` cardinality and `not-applicable` ordering.
