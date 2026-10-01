---
layout: docs
title: "chunk"
parent: "Partitioning functions"
grand_parent: "Array functions"
nav_order: 10
has_toc: false
permalink: /functions/array/partitioning/chunk/
tags:
  - functions
  - array/partitioning
generated: true
---

```
array<T> →
chunk(
    size: integer
) → array<array<T>>
```

Splits an array into consecutive, non-overlapping chunks of at most the specified size, preserving a final partial chunk. It resembles a count-based tumbling window but, unlike general sliding or hopping windows, has no separate step and always keeps the final partial chunk. It does not group items by inactivity or time. Returns `null` when the input cannot be evaluated.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `size` | `integer` | Yes | The strictly positive number of items in each chunk. |



## Value shape

- **Pipeline input:** `array<T>`
- **Returns:** `array<array<T>>`
- **Nullability:** The result is nullable when the pipeline input is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- Cardinality: `partitioned` <span class="semantics-info" title="Visited inputs are reorganized into groups or partitions." aria-label="Cardinality definition: Visited inputs are reorganized into groups or partitions.">i</span>
- Dependency: `partition` <span class="semantics-info" title="An output depends on the elements belonging to the same partition or key." aria-label="Dependency definition: An output depends on the elements belonging to the same partition or key.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

- **`size`:** Evaluated once in the enclosing context.



## Examples

{% raw %}
```expressif
{1, 2, 3} | chunk(2) → {{1, 2}, {3}}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/partitioning`  
**Aliases:** `chunk`
{: .member-reference }
