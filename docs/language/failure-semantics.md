---
title: Errors, null, and missing values
parent: Expressif language
nav_order: 14
description: Distinguish parse and binding errors, evaluation errors, null results, missing fields, and recoverable cases.
---

An Expressif expression can fail before it is executable, raise an error while it is evaluated, or evaluate successfully to `#null`. These outcomes are different.

## Three stages, three kinds of outcome

| Stage | What happens | Outcome |
|---|---|---|
| Parsing | Source text is read as Expressif syntax | Invalid syntax is rejected; no expression is created |
| Binding | Names, arguments, and expression contracts are resolved | An invalid call or unresolved construct is rejected; no executable expression is created |
| Evaluation | A bound expression receives an input value | The expression returns a value, including possibly `#null`, or raises an evaluation error |

Errors are not values. They do not enter the pipeline and cannot be passed to `coalesce` or another function.

## Parse and binding errors

A parse error means that the source is not valid Expressif syntax. An incomplete call such as this cannot be turned into an expression:

```expressif
10 | add(
```

A binding error occurs after parsing when the requested expression cannot be constructed. Examples include an unknown function, arguments that cannot be bound to the function's parameters, and an invalid expression contract.

```expressif
10 | unknown-function
```

An incorrect number of arguments is also a binding error. `add` requires a value argument, so this call cannot be bound:

```expressif
10 | add()
```

Both kinds of error occur before evaluation. Changing the input value cannot make the same parsed and bound request valid.

## Evaluation errors

An expression can parse and bind correctly but still encounter input for which its operation is undefined. That is an evaluation error when the operation's contract does not define a result value for the situation.

For example, selector-based coercion is valid syntax and can be bound without knowing the eventual pipeline input:

```expressif
coerce($1 -> :integer)
```

Evaluating this expression with the scalar input `42` raises an error because a tuple-position selector cannot be applied to a scalar. The binder can validate the selector and target type, but it cannot reject the expression based on an external input value that is supplied only during evaluation. The same expression can accept a tuple, so the structural mismatch is detected when the scalar value reaches `coerce`.

Evaluation stops at the failing stage. The error is not converted to `#null` and later pipeline stages do not receive it.

## `#null` is a value

`#null` is a normal Expressif value. It can be written as a literal, stored in a record or collection, passed through a pipeline, and returned by a function whose contract permits it.

```expressif
{name := "Ada"} | .score
```

This expression evaluates successfully to `#null`. Field access defines `#null` as its result when the requested field is missing.

A function can also return `#null` after successful evaluation. For example, an aggregate with no result for an empty input can define `#null` as its result:

```expressif
{} | min
```

Because `#null` is a valid result, its presence alone does not reveal why a previous function returned it.

## Failed coercion

Explicit `coerce` returns `#null` when a scalar value cannot be converted to the requested type:

```expressif
"Unknown" | coerce(:date)
```

This is a successful evaluation with a null result, not an evaluation error. It can be handled with `coalesce`:

```expressif
"Unknown" | coalesce(coerce(:date), "Invalid date")
```

which returns `"Invalid date"`.

There is no language-wide rule that every incompatible value or failed operation becomes `#null`. Implicit coercion and individual functions follow their documented contracts. A function may return `#null`, preserve its input, or raise an evaluation error depending on that contract.

## Missing is not a separate value

Expressif does not introduce an `absent` value alongside `#null`. Instead, the operation that accesses a field or member defines what happens when it is missing.

For ordinary field access, the distinction is:

| Input situation | Result |
|---|---|
| Field exists with a non-null value | That value |
| Field exists with `#null` | Successful `#null` result |
| Field does not exist | Successful `#null` result |
| Input does not expose named fields | Successful `#null` result |

Field access therefore does not preserve the distinction between an explicitly null field, a missing field, and an input without named fields. All three evaluate to `#null`. This is the documented contract of field access, not a language-wide rule for missing data or evaluation failures.

## Compatibility is decided before evaluation

Direct compatibility asks whether the current value can enter an expression without coercion. It does not inspect the expression's eventual result.

`guard(expression)`, or its `*expression` shorthand, uses that entry check. When the input is not directly compatible, the expression is skipped and the original input is returned unchanged:

```expressif
42 | *trim
```

returns `42`. This is neither an error nor a null result.

When the input is compatible, the guarded expression runs normally. A guard does not catch evaluation errors, and it does not replace a successful `#null` result with the original input.

## Compose recoverable results

Use ordinary value composition when a function documents `#null` as a recoverable result. `coalesce` selects the first non-null expression result:

```expressif
@value | coalesce(coerce(:integer), 0)
```

Use a guard when incompatible input should bypass an expression and remain unchanged. Neither construct handles parse errors, binding errors, or evaluation errors as values. Those errors must be corrected, prevented through validation, or handled by the application evaluating the expression.
