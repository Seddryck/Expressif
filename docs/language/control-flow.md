---
layout: docs
title: Control flow
parent: Language
nav_order: 65
---

`switch` tests predicates against the original input and evaluates the first matching result expression:

```expressif
82 | switch(
    less-than(50) => "failed",
    less-than(90) => "good",
    _ => "excellent"
) | upper
```

This produces `"GOOD"`. A selected result, including null, ends branch selection. Later predicates and expressions are skipped.

`try` evaluates each candidate against the original input, then tests its result:

```expressif
-5 | try(
    absolute => greater-than(10),
    neutral => is-negative,
    _ => 0
)
```

The first candidate is `5` and is rejected; the second candidate is the original `-5` and is accepted. Candidates are evaluated once. Acceptance depends entirely on a Boolean predicate: null can be accepted, and non-null values can be rejected. `coalesce` remains the first-non-null selection function.

Both forms use comma-separated branches. A branch may contain a pipeline or a nested control-flow call. `_ => expression` is an optional final fallback evaluated against the original input, without a predicate. Exhaustion without a fallback returns null. `switch` requires at least one predicate branch; `try` requires at least two alternatives, including at least one candidate/predicate pair. A fallback counts as an alternative.

Predicates must return Boolean values. Non-Boolean results and evaluation errors propagate immediately; neither function catches errors or attempts another branch after an error. The selected value continues through the surrounding pipeline.

The predicate in `try` receives the candidate as pipeline input. Root field references such as `^.threshold` retain the current object; relative access such as `.threshold` addresses the predicate input, following the language's normal field-access rules.
