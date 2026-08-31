---
title: Coercion in practice
parent: Expressif language
nav_order: 13
description: Use explicit coercion when values of the same source type can represent different kinds of information.
---

# Coercion in practice

Most Expressif expressions do not need explicit coercion.

Functions declare the types they accept, and Expressif normally performs compatible conversions implicitly. For most users and most expressions, this implicit coercion is sufficient.

Explicit coercion becomes useful when the content being processed can represent different types. A field might contain an age in one record and a birth date in another, even though both values arrive as text.

In that situation, coercion is not merely a conversion. It helps the expression determine how each value should be interpreted.

## Accepting either an age or a date

Suppose an array contains text representing either an age or a birth date:

```expressif
{"44", "2016-05-17"}
```

A birth date should be converted to a datetime and passed to `age`. If the value is not a datetime, the expression should try to interpret it as an integer instead:

```expressif
{"44", "2016-05-17"}
|> (coalesce(coerce(:datetime) | age, coerce(:integer)))
```

The result is:

```expressif
{44, 10}
```

`coalesce` evaluates its expressions from left to right and returns the first non-null result.

For `"44"`, datetime coercion fails:

```text
"44" → coerce(:datetime) → #null
```

The expression therefore tries integer coercion:

```text
"44" → coerce(:integer) → 44
```

For `"2016-05-17"`, datetime coercion succeeds and the resulting value is passed to `age`:

```text
"2016-05-17" → coerce(:datetime) → age → 10
```

The complete transformation is:

```text
"44"         → integer  → 44
"2016-05-17" → datetime → age → 10
```

This is the main use case for explicit coercion: content with variant types or meanings. The expression tries each supported interpretation and selects the first one that succeeds.

## Adding a fallback

Implicit coercion is often enough when only a successful result and a fallback are needed.

For example:

```expressif
coalesce(square-root, "Not numeric")
```

For a positive numeric value, `square-root` succeeds:

```expressif
16 → 4
```

For text that cannot be interpreted as numeric, it produces `#null`, so `coalesce` returns the fallback:

```expressif
"A" → "Not numeric"
```

There is a subtle problem, however. `"Not numeric"` does not accurately describe every failure:

```expressif
-16 → "Not numeric"
```

The value `-16` is numeric, but its real square root cannot be calculated. The fallback hides the difference between a coercion failure and a calculation failure.

## A misleading nested fallback

It may be tempting to distinguish those failures by nesting `coalesce` calls:

```expressif
coalesce(
    coalesce(coerce(:integer) | square-root, "Negative"),
    "Not numeric"
)
```

This does not work as intended.

The inner `coalesce` handles every null result from the complete pipeline:

```expressif
coerce(:integer) | square-root
```

That pipeline can produce `#null` because coercion failed or because the square-root calculation failed. In both cases, the inner `coalesce` returns `"Negative"`.

```expressif
"A" → "Negative"
-16 → "Negative"
16 → 4
```

Because the inner `coalesce` always returns either the calculated value or `"Negative"`, it never returns `#null`. The outer `"Not numeric"` fallback is therefore never selected.

The nested structure cannot identify which part of the pipeline produced null.

## Handling each stage separately

To distinguish the failures, handle conversion before attempting the calculation:

```expressif
coalesce(coerce(:integer), "Not numeric")
| coalesce(*square-root, "Negative")
```

The expression now distinguishes three outcomes:

```expressif
"A"  → "Not numeric"
-16  → "Negative"
16   → 4
```

The first `coalesce` handles coercion:

```expressif
coalesce(coerce(:integer), "Not numeric")
```

If the input cannot be interpreted as an integer, it returns `"Not numeric"`.

The second `coalesce` handles the calculation:

```expressif
coalesce(*square-root, "Negative")
```

The guarded call `*square-root` applies the function when the current value is compatible. It preserves the existing `"Not numeric"` fallback rather than attempting to calculate its square root.

For a negative integer, coercion succeeds but `square-root` produces `#null`. The second fallback then returns `"Negative"`.

```text
"A"  → #null → "Not numeric" → preserved
-16  → -16   → #null         → "Negative"
16   → 16    → 4
```

Explicit coercion is therefore most useful when an expression needs to control how variant content is interpreted or needs to distinguish conversion failure from failure in a later operation.
