---
layout: docs
title: "sort-by"
parent: "Sorting functions"
grand_parent: "Functions library"
nav_order: 180
has_toc: false
permalink: /functions/sorting/sort-by/
tags:
  - functions
  - sorting
generated: true
---

```
array →
sort-by(
    ...criteria: expression
) → array
```

Stably sorts an array by one or more typed criteria while preserving original elements.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `criteria` | `expression` | Variadic (one or more); no spread | Criteria in the form expression -> :type, optionally followed by direction and null-placement modifiers. |



## Value shape

- **Pipeline input:** `array<T>`
- **Returns:** `array<T>`
- **`criteria`:** Receives `T`.
- **Nullability:** The result is nullable when the pipeline input is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- **Cardinality:** `preserved`
- **Dependency:** `whole-input`
- **Ordering:** `reordered`

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

Visits each element of the array supplied as pipeline input to this sort-by call.

- **`criteria`:** Evaluated once per visited element in declaration order, with that element as context; .field reads its field and $0 reads its first tuple component.


## Behavior

Each criterion is evaluated once per source element in declaration order. :text selects ordinal comparison; integer, decimal, and numeric select numeric comparison; temporal types select their corresponding comparer. Direction defaults to ascending and null placement defaults to nulls last. Empty input returns an empty array without sampling an element. Invalid coercions become null keys, and equal keys preserve source order.



**Kind:** Function  
**Scope:** `sorting`  
**Aliases:** None
{: .member-reference }
