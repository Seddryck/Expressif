---
title: Shorthands
subtitle: Concise forms supported by the Expressif parsers
tags: [syntax, shorthand]
keywords: [shorthand, field, map, interval, variable, property, index, spread]
---

Expressif provides concise syntax for common pipeline, field, parameter, interval, and predication operations.

## Mapping each array item: `|> (...)`

`|> (...)` is shorthand for `| map(...)`. The mapped expression must be enclosed in parentheses:

```text
[customers] |> (.name | upper) | reverse
```

This is equivalent to:

```text
[customers] | map(field(name) | upper) | reverse
```

The ordinary `|` following `)` resumes operations on the complete mapped array.

## Reading an incoming field: `.name`

`.name` is shorthand for `field(name)` and reads from the current input of that expression:

```text
[customers] |> (.name)
[customers] | filter(.active)
record(customer-name := .name, country := .country)
```

Field names may contain letters, digits after the first character, `_`, `-`, and `+`. There must be no whitespace after the dot. Use the long form for dynamic or quoted names:

```text
field([requested-field])
field("customer name")
```

Compose nested access as a pipeline; dotted paths are not supported:

```text
.address | .city
```

## Parameters and incoming values

| Shorthand | Meaning |
|---|---|
| `@name` | Context variable named `name` |
| `[name]` | Field `name` on the persistent current object |
| `#2` | Item at zero-based index `2` |
| `...` | Complete incoming value |

Inside `record(...)`, a standalone `...` spreads the incoming record. On the right of `:=`, it embeds the complete incoming value instead.

## Interval shorthands

Intervals have symbolic concise forms:

| Shorthand | Interval |
|---|---|
| `(+)` | Strictly positive |
| `(0+)` | Positive or zero |
| `(-)` | Strictly negative |
| `(0-)` | Negative or zero |
| `(>5)` / `(>=5)` | Greater than / greater than or equal to `5` |
| `(<5)` / `(<=5)` | Less than / less than or equal to `5` |

The word forms `(positive)`, `(absolutely-positive)`, `(negative)`, and `(absolutely-negative)` are also accepted.

## Predication operators

`|?` starts a predication from an explicit input, `!` negates a predicate, and `|AND`, `|OR`, and `|XOR` combine predicates:

```text
@age |? !less-than(18) |AND less-than(65)
```

These operators are parser syntax; they do not register additional functions or predicates.
