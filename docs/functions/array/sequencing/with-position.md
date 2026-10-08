---
layout: docs
title: "with-position"
parent: "Sequencing functions"
grand_parent: "Array functions"
nav_order: 70
has_toc: false
permalink: /functions/array/sequencing/with-position/
tags:
  - functions
  - array/sequencing
generated: true
---

```
array<T> →
with-position() → array<tuple<integer, T>>
```

Returns each input item paired with its zero-based position as a tuple in `(position, value)` order. Preserves input order and cardinality. Position terminology distinguishes sequence locations from indexes used to accelerate searches. Returns `null` when the input cannot be evaluated.



## Parameters



This function has no parameters.



## Examples

{% raw %}
```expressif
{"a", "b", "c"} | with-position | value-at(2) | tuple-first → 2
```
{% endraw %}

## Value shape

- Pipeline input: `array<T>`
- Returns: `array<tuple<integer, T>>`
- Nullability: The result is nullable when the pipeline input is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- Cardinality: `preserved` <span class="semantics-info" title="The output contains the same number of elements as the visited input." aria-label="Cardinality definition: The output contains the same number of elements as the visited input.">i</span>
- Dependency: `per-element` <span class="semantics-info" title="An output element depends only on its corresponding visited input element." aria-label="Dependency definition: An output element depends only on its corresponding visited input element.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.

**Kind:** Function  
**Scope:** `array/sequencing`  
**Aliases:** `with-position`
{: .member-reference }
