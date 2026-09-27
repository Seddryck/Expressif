# Logical-plan binder spike

This spike starts from PR 1217 commit `0819a8cda1e24cbd7034bd985661e718bc4af5be`.
It reconstructs runtime binding objects from `LogicalPlan` and invokes the existing
runtime factory without consulting the syntax tree. The adapter is intentionally
internal: its purpose is to identify what must be resolved before exposing a
supported plan-binding API.

## What works

The spike binds ordinary open and closed pipelines, JSON-round-tripped plans,
input bindings, literals, value constructors, field and tuple references,
predicates, control flow, variables, callable references, sorting criteria,
intervals, coercion specifications, record construction, and special scalar
values. Every documented expression which the current planner can produce can
also be constructed by the spike.

## Planning blockers

The catalog and the bound parameter model do not currently describe the same
call shape for every operator. Consequently, the planner rejects documented,
valid expressions before a plan binder can see them.

- `subtract` documents `times` as required although the runtime supports its
  omission. This prevents planning nested `subtract` calls in `chunk-while` and
  `map-over` examples.
- `power`, `nth-root`, `change-of-hour`, `change-of-minute`, `change-of-month`,
  `change-of-second`, `change-of-year`, `local-to-utc`, `utc-to-local`, and
  `catholic-calendar` document no parameters although their documented examples
  supply parameters.
- `swap` documents both positions as required although the runtime and summary
  define defaults.
- Entry-shaped operators do not fit generic name-to-parameter normalization.
  `transform-as`, `put`, `put-present`, and `put-absent` accept caller-defined
  argument names. `with` is bound as one `WithDefinitionParameter`, while the
  catalog exposes separate `projections` and `body` parameters.

The catalog-example audit in `LogicalPlanBinderTest` records these failures and
ensures that all other documented examples can be constructed from their plans.

## Logical-plan contract gaps

### Operator identity is incomplete

The function catalog used by `LogicalPlannerFactory` does not include predicates.
Predicate calls therefore receive synthetic `argument-N` parameters, `any`
types, and no operator kind. The .NET spike can fall back to positional runtime
binding, but a portable backend cannot validate such a call from the plan alone.
`PlannerFunctionDescriptor` should carry a stable operator kind, and planning
must resolve functions, predicates, and accumulators through one vocabulary.

The planner also currently accepts unresolved operator names by creating a
synthetic descriptor. An imported plan therefore cannot distinguish a declared
intrinsic from a misspelled or unavailable implementation. Intrinsics need a
declared vocabulary and imported plans need catalog validation.

### Pipeline source intent can be ambiguous

An explicit value constructor and an open transformation can lower to the same
first `LogicalCall`. For example, a tuple or record call can be either an
explicit source or an operation applied to the surrounding input. The spike can
usually infer the intended form from the enclosing parameter contract, but that
is a heuristic. It cannot preserve the exact `BindClosed` rejection contract for
plans such as `array(1)` and `{1}`, which are identical.

This does not require restoring `OpenExpressionParameter` and
`InputExpressionParameter` as plan concepts. A smaller contract is sufficient:
identify whether a pipeline has an explicit source, or define and test a
dependency rule which determines that fact unambiguously.

### Structural calls are context-sensitive

`record` represents both record literals and `RecordDefinitionParameter` values.
The spike distinguishes them using the enclosing catalog parameter kind
(`entry`). Likewise, calls such as `array`, `tuple`, `pair`, `grouping`, and
`dictionary` can be value nodes or executable operators. This is bindable while
catalog metadata is complete, but it makes imported-plan validation dependent
on that metadata and should be made an explicit part of the plan contract.

## Backend work, not plan data

- Provide a supported public binding API for `LogicalPlan` and integrate it into
  `ExpressionFactory` after planning.
- Validate catalog compatibility, operator availability, arguments, omissions,
  and intrinsic shapes before runtime construction.
- Decide whether automatic coercion insertion remains a .NET backend pass. The
  existing runtime factory successfully evaluates the typed pipeline exercised
  by the spike, but coercion behavior needs broader parity coverage.
- Preserve source locations separately if plan-binding diagnostics must point
  back to source text.
