---
layout: docs
title: "lead"
parent: "Sequencing functions"
grand_parent: "Array functions"
nav_order: 30
has_toc: false
permalink: /functions/array/sequencing/lead/
tags:
  - functions
  - array/sequencing
generated: true
---

```
array<T> →
lead() → array<nullable<T>>
```

Returns the next value for each input element. The last output value is `null` because there is no next element. Preserves input cardinality (one output item per input item). Returns `null` when the input is not an enumerable or is a string.



## Parameters



This function has no parameters.



## Value shape

- Pipeline input: `array<T>`
- Returns: `array<nullable<T>>`
- Nullability: The result is nullable when the pipeline input is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- Cardinality: `preserved` <span class="semantics-info" title="The output contains the same number of elements as the visited input." aria-label="Cardinality definition: The output contains the same number of elements as the visited input.">i</span>
- Dependency: `unknown` <span class="semantics-info" title="Dependency is structurally relevant but cannot be declared more precisely." aria-label="Dependency definition: Dependency is structurally relevant but cannot be declared more precisely.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.





## Examples

{% raw %}
```expressif
{1, 2, 3} | lead → {2, 3, #null}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/sequencing`  
**Aliases:** `array-to-lead`
{: .member-reference }
