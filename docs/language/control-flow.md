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

## Conditional transformations

`predicate ?> expression` tests the original input. When true, it evaluates the expression; when false, it returns the original input without evaluating the expression.

`expression <? predicate` evaluates the expression once, then tests its candidate result. When true, it returns the candidate; when false, it returns the original input. Returning the input does not undo effects of an expression already evaluated.

```expressif
-12 | is-negative ?> absolute
-5 | absolute <? greater-than(10)
```

These produce `12` and `-5`, respectively. Both operators propagate errors immediately and require Boolean predicates. Null receives no special handling: `#null <? is-null` accepts null even for a non-null original input.

The conditional operators bind more tightly than the surrounding `|`. Use parentheses for multi-stage operands:

```expressif
-10 | (absolute | add(5)) <? greater-than(20) | add(2)
```

The candidate `15` is rejected, so the original stage input `-10` is restored and `add(2)` produces `-8`. Without the parentheses, `absolute | add(5) <? greater-than(20)` applies the conditional only to `add(5)`; rejection restores `10`.

The two conditional operators have equal precedence. Chaining them requires explicit grouping, for example `(is-negative ?> absolute) <? greater-than(10)`. Ungrouped chains are invalid. Commas and `=>` separate branches at their current nesting level; line breaks do not affect grouping.

```expressif
-10 | try(
    (absolute | add(5)) <? greater-than(20) => is-positive,
    _ => 0
)
```

The inner conditional returns `-10` after rejection. The outer `try` predicate rejects that result, so the fallback returns `0`.

| Construct | Predicate input | On rejection |
| --- | --- | --- |
| `switch` | Original input | Try the next branch. |
| `try` | Candidate result | Try the next candidate against the original input. |
| `?>` | Original input | Return the original input without evaluating the expression. |
| `<?` | Candidate result | Return the original input after evaluating the expression. |

`switch` and `try` return null when alternatives are exhausted without a fallback. Conditional operators preserve the original input instead. Consequently, wrapping conditional operators in `coalesce` does not generally reproduce `try`.
