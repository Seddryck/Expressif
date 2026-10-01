---
layout: docs
title: "dense-rank-by"
parent: "Sorting functions"
grand_parent: "Functions library"
nav_order: 110
has_toc: false
permalink: /functions/sorting/dense-rank-by/
tags:
  - functions
  - sorting
generated: true
---

```
array<T> →
dense-rank-by(
    ...criteria: expression
) → grouping<integer, T>
```

Groups array elements by their one-based dense rank using typed criteria.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `criteria` | `expression` | Variadic (one or more); no spread | Criteria in the form expression -> :type, optionally followed by direction and null-placement modifiers. |



## Value shape

- **Pipeline input:** `array<T>`
- **Returns:** `grouping<integer, T>`
- **`criteria`:** Receives `T`.
- **Nullability:** The result is nullable when the pipeline input is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- Cardinality: `partitioned` <span class="semantics-info" title="Visited inputs are reorganized into groups or partitions." aria-label="Cardinality definition: Visited inputs are reorganized into groups or partitions.">i</span>
- Dependency: `whole-input` <span class="semantics-info" title="An output depends on the complete visited input." aria-label="Dependency definition: An output depends on the complete visited input.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

Visits each element of the array supplied as pipeline input to this dense-rank-by call.

- **`criteria`:** Evaluated once per visited element in declaration order, with that element as context; .field reads its field and $0 reads its first tuple component.


## Behavior

Builds a SortTable using the same criterion lowering as sort-by and delegates to dense-rank. Comparer-equal keys share a group. Empty input returns an empty grouping. Invalid coercions become null keys.



**Kind:** Function  
**Scope:** `sorting`  
**Aliases:** None
{: .member-reference }
