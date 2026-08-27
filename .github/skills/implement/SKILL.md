---
name: implement
description: "Implement a scaffolded Expressif function, predicate, or accumulator for the post-v2 architecture, including typed contracts, binder integration, metadata consistency, and conformance coverage. Use only when documentation metadata and conformance cases already exist."
---

# /implement

Implement an operator whose documentation metadata and conformance cases were already created by `/scaffold`.

This skill targets the architecture after the `next-major` development line. Do not preserve or recreate legacy parser/factory patterns from `main`.

Supported operator kinds are function, predicate, and accumulator.

Follow `AGENTS.md` for issue, branch, worktree, commit, push, and pull-request requirements. This skill does not replace that workflow.

## Establish the contract

Locate the operator by canonical name or alias in the matching file:

* `docs/_data/function.json`;
* `docs/_data/predicate.json`;
* `docs/_data/accumulator.json`.

Read its YAML under `conformance/<kind>/<scope-lower>/`. Stop if either artifact is absent or ambiguous: scaffolding is incomplete.

Metadata is authoritative for public names, aliases, scope, summaries, parameter names, semantic types, optionality, input type, and output type. Existing binding and runtime abstractions are authoritative for construction and evaluation mechanics.

Before editing, inspect the nearest implementation family, its contracts and registration, the bound parameter representations consumed by the binder or factory, introspection tests, and the matching conformance test class. Do not derive a universal C# shape solely from semantic scope.

## Select an implementation path

Choose the narrowest path that represents the operator correctly.

### Closed typed operator

Use this path when input and output types are known at binding time.

Expose the most precise available `IFunction<TIn, TOut>` contract, normally through an existing typed family base or `Function<TIn, TOut>`. An operator may expose multiple closed contracts when several input types are intentionally supported.

Keep the untyped `IFunction.Evaluate(object?)` path only as the established compatibility bridge. Do not make it the primary contract for an otherwise closed operator.

### Dynamic or polymorphic operator

Use this path when output depends on information that one closed contract cannot represent, such as an input-preserving function, a selected record field, or child-expression outputs.

Use the repository's semantic contract representation. Do not publish an unexplained `any -> any` contract or add a name-based introspection exception. Add tests proving that the dynamic contract is intentional.

### Binder-integrated operator

Use this path when ordinary constructor binding cannot express deferred or short-circuit evaluation, nested expressions, lexical or contextual access, structured entries, spread, variadic parameters, type inference, or function-specific binding validation.

Keep the layers separate:

1. `Expressif.Syntax` represents parsed syntax;
2. `Expressif.Bindings` represents validated, bound calls and parameters;
3. the runtime function evaluates already-bound behavior.

Extend the existing binder and factory abstractions. Do not introduce `IFunctionBuilder`, parser-aware runtime classes, or a parallel construction registry unless the repository has deliberately adopted such an abstraction.

Binding owns parameter-shape validation, canonical and named argument resolution, spread or variadic expansion, nested-expression binding, type inference, and deferred evaluator creation. Runtime code owns evaluation semantics only.

### Predicate

Implement a predicate as a Boolean-returning function contract. Reuse the shared callable pipeline and preserve the explicit Boolean output requirement expected by Boolean consumers such as `filter` and logical composition.

Use predicate-specific registration or bases only where they remain part of the post-v2 registry. Do not duplicate function resolution, chaining, coercion, or argument binding for predicates. Check canonical predicate naming and retain compatibility names only when metadata declares them as aliases.

### Accumulator

Inherit from `BaseAccumulator` and preserve the lifecycle:

```csharp
public override void Initialize();
public override void Accumulate(object? item);
public override object? GetValue();
```

Keep mutable aggregation state in private fields and reset all per-run state in `Initialize()`. Preserve public parameterless construction while `AccumulatorFactory` requires it.

## Parameters and constructors

Match constructor parameters to canonical metadata order and semantic types. Runtime-evaluated parameters normally use the provider representation established by the nearest family. Do not blindly wrap every parameter in `Func<T>`: bound expressions, variadic arguments, structured definitions, enums, and factory-provided services may need different representations.

Follow the existing optional-parameter convention and preserve metadata defaults. Named argument binding must reorder values into canonical order and omit optional parameters without relying on incidental CLR parameter names.

For variadic parameters, use the shared variadic bound representation. Preserve declaration order, spread position, common-type inference, and empty-input behavior. Do not hard-code a reusable capability solely for one function.

## Evaluation state

Bound expressions and functions must remain reusable across concurrent evaluations.

Use immutable evaluation context for shared runtime values and the evaluation frame for input, ambient values, lexical state, and per-evaluation observation state. Do not mutate shared context for lexical binding or retain per-call state on a bound expression.

Evaluate deferred arguments only when semantics require them. Preserve declaration order and short-circuit behavior.

## Coercion and composition

Declare precise contracts so the binder can compose adjacent stages and select coercions from `CoercionRegistry`.

Do not add ad hoc casting to make an incompatible pipeline succeed. Supported transitions bind through registered coercion contracts; unsupported transitions fail during binding with a language-level diagnostic.

Coercion functions use established `TryCast` semantics, register every supported typed input contract, and verify introspection for all exposed contracts.

## Documentation and registration

Copy XML summary and parameter text exactly from metadata, escaping XML characters where necessary.

Apply canonical name, aliases, visibility, and scope from metadata. The operator must be discoverable through its registry and consistent with the embedded catalog and introspection model.

Validate canonical-name and alias lookup, parameter semantic types and optionality, input and output contracts, callable-name collisions, and explicit dynamic-contract reasons where applicable.

## File placement

Place implementation beside the nearest cohesive family. If none fits, create a descriptive `.cs` file under the appropriate `Expressif/Functions/<Scope>`, `Expressif/Predicates/<Scope>`, or `Expressif/Accumulators` directory.

Mirror that organization in `Expressif.Testing`. Do not create files literally named `function` or `accumulator`, and do not introduce a new scope base solely for one operator.

Keep binding changes with binding/factory infrastructure and runtime behavior with the operator.

## Conformance coverage

Add or update a test anchor in `Expressif.Testing` using `[Conformance]`.

Create one method for each YAML `tests[].id`, converting dot- and kebab-separated segments to PascalCase joined by underscores. The conformance loader creates NUnit cases from each YAML `cases` entry.

Method arguments follow loader output:

1. input;
2. YAML parameters, or one compatible array when packed;
3. context variables in deterministic key order;
4. expected value.

Choose CLR types compatible with loader normalization and the operator contract. Keep `expected` last. Exercise typed evaluation when proving a closed contract.

Add focused tests beyond conformance only for behavior YAML cannot express adequately: binding diagnostics and source spans; named, optional, spread, or variadic binding; inference and coercion; deferred or short-circuit evaluation; evaluation-frame isolation and concurrency; registry collisions; or semantic introspection.

Accumulator tests directly exercise `Initialize`, repeated `Accumulate`, and `GetValue` unless YAML explicitly specifies higher-level composition.

## Validation

Before completion:

1. confirm metadata, registration, and introspection agree;
2. confirm every regular function exposes intended closed contracts or an explicit dynamic contract;
3. confirm binder-integrated behavior consumes bound representations, not parser types;
4. confirm predicates expose an explicit Boolean result;
5. confirm accumulator state resets completely;
6. run relevant conformance and focused tests;
7. run the solution build;
8. complete the repository workflow from `AGENTS.md`.

Report implementation path, contracts, files changed, metadata source, tests run, and intentionally dynamic behavior.
