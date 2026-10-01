---
layout: docs
title: "lag"
parent: "Sequencing functions"
grand_parent: "Array functions"
nav_order: 20
has_toc: false
permalink: /functions/array/sequencing/lag/
tags:
  - functions
  - array/sequencing
generated: true
---

```
array<T> →
lag() → array<nullable<T>>
```

Returns the previous value for each input element. The first output value is `null` because there is no previous element. Preserves input cardinality (one output item per input item). Returns `null` when the input is not an enumerable or is a string.



## Parameters



This function has no parameters.



## Examples

{% raw %}
```expressif
{1, 2, 3} | lag → {#null, 1, 2}
```
{% endraw %}

## Value shape

- Pipeline input: `array<T>`
- Returns: `array<nullable<T>>`
- Nullability: The result is nullable when the pipeline input is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- Cardinality: `preserved` <span class="semantics-info" title="The output contains the same number of elements as the visited input." aria-label="Cardinality definition: The output contains the same number of elements as the visited input.">i</span>
- Dependency: `prefix` <span class="semantics-info" title="An output at a position depends on the visited prefix ending at that position." aria-label="Dependency definition: An output at a position depends on the visited prefix ending at that position.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.

**Kind:** Function  
**Scope:** `array/sequencing`  
**Aliases:** `array-to-lag`
{: .member-reference }
