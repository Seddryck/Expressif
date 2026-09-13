# Logical plan JSON format

Expressif logical plans are a language-level interchange format. They contain canonical operator and parameter descriptors, semantic literal types, argument context depth, evaluation rules, spread markers, and omission contracts. They never contain CLR types, assemblies, delegates, or backend implementation objects.

The top-level object identifies the format as `expressif.logical-plan`, declares its integer `version`, identifies the compatible catalog line with `catalogCompatibility`, and contains a pipeline in `plan`. Version 1 is described by [`logical-plan.schema.json`](../_data/logical-plan.schema.json).

## Compatibility

Readers must reject an unknown format name, plan version, node kind, or catalog compatibility level. A reader may accept added optional fields only in a later format version whose compatibility rules explicitly allow them; version 1 objects reject undeclared fields. Existing fields and node kinds do not change meaning within a format version.

Writers emit canonical operator names and properties in deterministic order. The same normalized plan therefore produces byte-for-byte identical compact JSON. Consumers should use semantic `type` values to interpret literals and must not infer host-language runtime types.

## Nodes

A `pipeline` contains ordered `items`. A `call` contains its canonical operator descriptor, context depth, and arguments in canonical parameter order. Each argument records whether it was explicitly supplied, whether it spreads its value, and either its value or its omission behavior. A `literal` pairs an Expressif semantic type with its JSON representation; temporal and duration values use invariant strings.

References and structured value constructors use calls rather than private node kinds. For example, `.age` uses `field`, `$1` uses `tuple-at`, and array and tuple literals use `array` and `tuple`.
