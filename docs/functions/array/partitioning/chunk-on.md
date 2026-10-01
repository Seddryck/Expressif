---
layout: docs
title: "chunk-on"
parent: "Partitioning functions"
grand_parent: "Array functions"
nav_order: 30
has_toc: false
permalink: /functions/array/partitioning/chunk-on/
tags:
  - functions
  - array/partitioning
generated: true
---

```
array<T> →
chunk-on(
    position: integer
) → tuple<array<T>, array<T>>
```

Splits an array on a zero-based boundary and returns the elements before and from that position as a tuple. Positions beyond the end use the end boundary. Returns `null` when the position is negative or the input cannot be evaluated.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `position` | `integer` | Yes | The zero-based boundary position; the element at this position belongs to the right chunk. |



## Value shape

- **Pipeline input:** `array<T>`
- **Returns:** `tuple<array<T>, array<T>>`
- **Nullability:** The result is nullable when the pipeline input is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- Cardinality: `partitioned` <span class="semantics-info" title="Visited inputs are reorganized into groups or partitions." aria-label="Cardinality definition: Visited inputs are reorganized into groups or partitions.">i</span>
- Dependency: `partition` <span class="semantics-info" title="An output depends on the elements belonging to the same partition or key." aria-label="Dependency definition: An output depends on the elements belonging to the same partition or key.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

- **`position`:** Evaluated once in the enclosing context.


## Behavior

`chunk-on` materializes the input and returns `T(before, from-position)`. Positions beyond the input cardinality use the end boundary, so the right chunk is empty. Negative positions return `null`.



## Examples

{% raw %}
```expressif
{10, 20, 30, 40} | chunk-on(2) → T({10, 20}, {30, 40})
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/partitioning`  
**Aliases:** None
{: .member-reference }
