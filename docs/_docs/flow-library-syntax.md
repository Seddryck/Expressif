---
title: Flow library and syntax
subtitle: Pipeline flow operators and record shaping syntax
tags: [functions, flow, syntax]
keywords: [record, field, current-object, incoming-value, spread, ...]
---

The flow library and syntax define how data moves through a pipeline and how values are read or reshaped at each stage.

## Core concepts

The following constructs are complementary:

* `[name]`: reads a field from the persistent current object.
* `field(name)`: reads a field from the value entering the current stage.
* `record(...)`: constructs a new ordered record value.
* `...`: represents the full incoming value.

## Current object versus incoming value

`[name]` always reads from the current object associated with the execution context.

`field(name)` always reads from the value entering the current pipeline stage.

Example:

```text
[customer]
| record(
    customerName := field(name),
    requestedBy := [name]
)
```

In this expression:

* `field(name)` reads from `[customer]` (the incoming stage value).
* `[name]` reads from the current object.

## The incoming-value expression: ...

`...` can be used in two positions with different behavior.

### 1) As an independent record entry (spread)

When used as an entry inside `record(...)`, `...` expands the incoming value into the result record.

```text
record(
    ...,
    processed := true
)
```

If the incoming value is a record-like value, its fields are inserted at that position.

Entries are applied from left to right; on name collisions, the last value wins.

### 2) As a named field value (embed)

When used on the right side of `:=`, `...` stores the incoming value as-is, without expansion.

```text
record(
    original := ...,
    normalized := upper
)
```

## Spreading non-record values

If `...` is used as a spread entry and the incoming value is not record-like, Expressif adds it using a generated field name:

```text
__NONAME_0
```

If that name already exists, the next available index is used (`__NONAME_1`, `__NONAME_2`, ...).

## record function

`record(...)` creates a new ordered record from its entries.

Properties:

* Field order is preserved.
* Explicit duplicate field declarations are rejected.
* `null` results are retained as present fields.
* The output of `record(...)` does not replace the current object.

## field function

`field(name)` retrieves a field from the incoming pipeline value.

It is useful inside `record(...)` to transform fields from the current stage input without relying on the current object.
