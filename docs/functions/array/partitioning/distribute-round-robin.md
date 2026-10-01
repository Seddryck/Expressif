---
layout: docs
title: "distribute-round-robin"
parent: "Partitioning functions"
grand_parent: "Array functions"
nav_order: 70
has_toc: false
permalink: /functions/array/partitioning/distribute-round-robin/
tags:
  - functions
  - array/partitioning
generated: true
---

```
array<T> →
distribute-round-robin(
    count: integer
) → array<array<T>>
```

Distributes successive array values cyclically among a requested number of output arrays. Returns `null` when the count is not strictly positive or the input cannot be evaluated.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `count` | `integer` | Yes | Specifies the strictly positive number of output arrays. |



## Examples

{% raw %}
```expressif
{1, 2, 3, 4, 5} | distribute-round-robin(2) → {{1, 3, 5}, {2, 4}}
```
{% endraw %}

## Value shape

- Pipeline input: `array<T>`
- Returns: `array<array<T>>`
- Nullability: The result is nullable when the pipeline input is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- Cardinality: `partitioned` <span class="semantics-info" title="Visited inputs are reorganized into groups or partitions." aria-label="Cardinality definition: Visited inputs are reorganized into groups or partitions.">i</span>
- Dependency: `partition` <span class="semantics-info" title="An output depends on the elements belonging to the same partition or key." aria-label="Dependency definition: An output depends on the elements belonging to the same partition or key.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

- **`count`:** Evaluated once in the enclosing context.


## Behavior

The value at zero-based input position `i` is assigned to output array `i modulo count`. The result always contains exactly `count` arrays, including empty trailing arrays when the count exceeds the input cardinality. Relative input order is preserved within every output array. Empty input returns `count` empty arrays.

**Kind:** Function  
**Scope:** `array/partitioning`  
**Aliases:** None
{: .member-reference }
