---
name: scaffold
description: "Scaffold documentation metadata and conformance cases for a new post-v2 Expressif function, predicate, or accumulator before implementation. Use when defining a new operator; do not use for implementing an existing scaffold."
---

# /scaffold

Define the public contract for a new operator and create the catalog record and conformance YAML consumed by `/implement`.

This skill targets the architecture after the `next-major` development line. Do not preserve legacy metadata or parser conventions from `main`.

Supported kinds are function, predicate, and accumulator.

Follow `AGENTS.md` for issue, branch, worktree, commit, push, and pull-request execution. Scaffolding may propose a commit message, but it does not independently authorize or forbid those Git operations.

## Gather the contract

Collect enough information to make behavior and typing unambiguous:

* kind, canonical kebab-case name, aliases, visibility, and semantic scope;
* concise summary and special-value behavior;
* semantic input and output contracts;
* parameters in canonical order, including name, semantic type, summary, optionality, omission/default semantics, and variadic status;
* cardinality, evaluation order, materialization, short-circuit, or stateful behavior when relevant;
* representative examples and expected results.

Use the semantic type vocabulary and catalog shape currently established on the post-v2 development line. Inspect the current catalogs, conformance schema, and a nearby operator before proposing fields or values. Do not use a hard-coded scope list when the catalog supports additional scopes such as Boolean, Record, or IO.

## Semantic contract kinds

Classify the operator before writing metadata.

### Closed contract

Use concrete semantic input and output types when they are known at binding time. Record every intentionally supported input contract when an operator, especially a coercion, accepts multiple input types.

### Dynamic or polymorphic contract

When output cannot be described by one fixed type, record the relationship represented by the repository's current semantic contract model, for example:

* output preserves the input type;
* output depends on a selected field;
* output depends on child-expression outputs;
* output is the inferred common type of variadic arguments.

Do not replace a known relationship with unexplained `any` metadata.

### Predicate contract

Predicates are Boolean-returning callable functions. Record their semantic input and Boolean output and follow the repository's canonical question-style naming convention. Compatibility spellings belong in aliases, not in the canonical name.

### Accumulator contract

Describe the accumulated item type, result type, initial/empty result, null handling, and order sensitivity. Do not force function-style failure or cardinality semantics onto accumulators.

## Names and collisions

Canonical names, aliases, and parameter names use lowercase kebab-case unless the current language model explicitly permits another form.

Keep canonical name separate from aliases. Do not require aliases to repeat the canonical name and default to an empty alias list when no compatibility or shorthand alias is intended.

Reject duplicate canonical names or aliases according to the shared post-v2 callable-resolution policy. Check across function and predicate catalogs when both participate in the same public callable namespace. Also reject duplicate parameter names.

Named arguments bind against canonical metadata names. Preserve parameter order for positional calls even when named arguments can be reordered at the call site.

## Parameters

Every parameter record includes the fields required by the current catalog schema, including semantic `Type`, `Optional`, and documentation summary.

For an optional parameter, record its default value when the schema represents one. Otherwise document the defined omission behavior; do not invent a CLR default.

For a variadic parameter:

* mark it with the catalog's variadic field;
* record its element/expression semantic type;
* define minimum cardinality and empty invocation behavior;
* state whether spread is accepted and how it preserves argument order;
* define output inference for homogeneous, heterogeneous, and empty arguments.

Only the final positional parameter may be variadic unless the language model explicitly supports another shape.

## Documentation

Write summaries as concise present-tense descriptions of observable behavior. Parameter summaries describe the public role of the parameter rather than CLR or binder mechanics.

Document null, empty, blank, invalid-input, and binding behavior only as semantics require. Do not normalize every failure to null:

* predicates have explicit Boolean behavior;
* functions may preserve, transform, materialize, or reject special values;
* unsupported calls may fail during binding with a diagnostic;
* accumulators define their own empty and null lifecycle behavior.

End prose sentences with punctuation and preserve stable terminology used by adjacent catalog entries.

## Catalog output

Map kind to:

* function: `docs/_data/function.json`;
* predicate: `docs/_data/predicate.json`;
* accumulator: `docs/_data/accumulator.json`.

Emit the complete record required by the current post-v2 schema. For functions this includes at least `Name`, `IsPublic`, `Aliases`, `Scope`, `Input`, `Output`, `Summary`, and typed `Parameters`. Emit contract-dependency, default, or variadic fields when applicable.

Do not append an incomplete record merely because older entries omit newer semantic fields. Preserve existing formatting and ordering without reordering unrelated entries.

## Conformance output

Create `conformance/<kind-plural>/<scope-lower>/<name>.yaml` and validate it against `conformance/conformance.schema.json`.

Use the schema fields `suite`, `kind`, `operator`, and `tests`. Each test has an ID and one or more cases. Each case contains the schema-required input and expected value plus parameters or context when needed.

Use lowercase dot-separated test and case IDs whose segments describe behavior or invocation form. Do not require parameter names in every test ID. Case IDs extend the test ID with a meaningful input/type and scenario suffix.

Select cases from semantic partitions rather than a fixed count. Cover applicable behavior such as:

* ordinary representative values and boundaries;
* each optional/defaulted invocation form;
* named and positional equivalence when it is part of the operator contract;
* empty, single, multiple, heterogeneous, nested, and spread forms for variadic operators;
* every supported typed input contract for coercions;
* short-circuit or declaration-order behavior;
* accumulator empty, null, repeated, and order-sensitive behavior;
* null, empty, and blank only when meaningful for the declared input and semantics.

Use `(null)`, `(empty)`, and `(blank)` for the corresponding Expressif special input values. YAML null is an empty `expected:` value. Quote strings when YAML could reinterpret language syntax, arrays, records, Booleans, or special tokens.

The conformance loader supplies method arguments in this order: input, case parameters (or one packed parameter array), context variables ordered by key, then expected. Structure cases so `/implement` can create a compatible anchor.

Evaluation conformance records observable results. Parser or binder diagnostics that the conformance schema cannot represent belong in focused implementation tests, but scaffold documentation must still state the invalid form and expected diagnostic behavior.

If expected results cannot be derived unambiguously, ask for concrete examples before writing. Never invent semantics from an operator name alone.

## Commit message proposal

After the contract is settled, propose a Conventional Commit message describing the eventual completed change. Treat it as a handoff suggestion that `/implement` or the final workflow may retain or refine after implementation.

Prefer the change's nature over the fact that metadata was scaffolded:

* new operator: `feat(<scope>): add <behavior>`;
* correction to an existing operator definition: `fix(<scope>): <corrected behavior>`;
* metadata-only correction: `docs(data): <description>`;
* conformance-only correction: `test(conformance): <description>`.

Do not run `git commit`, create a branch, or alter the repository workflow solely because a message was proposed. Follow the user's requested scope and `AGENTS.md`.

## Validation before writing

Confirm that:

1. catalog and conformance targets do not already exist unless an update was requested;
2. names and aliases satisfy canonical naming and collision rules;
3. semantic input, output, and parameter types are complete;
4. dynamic relationships, optional defaults, and variadic behavior are explicit where applicable;
5. summaries and expected results agree;
6. YAML conforms to the schema and can map to a conformance test method;
7. the proposed commit message describes the eventual completed change.

Show a preview when the user requests one or when unresolved choices require confirmation. Otherwise apply the scoped metadata and conformance edits without an unconditional confirmation gate.

## Completion report

Report:

* operator kind, name, and semantic contract;
* catalog and conformance files changed;
* optional, dynamic, or variadic semantics recorded;
* validation performed;
* proposed commit message;
* whether any Git commit was actually created.
