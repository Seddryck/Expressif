---
layout: docs
title: "to-tuple"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 90
has_toc: false
permalink: /functions/array/to-tuple/
tags:
  - functions
  - array
generated: true
---

```
array<T> →
to-tuple() → variadic-tuple<T>
```

Returns a tuple containing the input array's elements in order. Returns `null` when the input is not an array.



## Parameters



This function has no parameters.



## Value shape

- **Pipeline input:** `array<T>`
- **Returns:** `variadic-tuple<T>`
- **Nullability:** The result is nullable when the pipeline input is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- Cardinality: `preserved` <span class="semantics-info" title="The output contains the same number of elements as the visited input." aria-label="Cardinality definition: The output contains the same number of elements as the visited input.">i</span>
- Dependency: `per-element` <span class="semantics-info" title="An output element depends only on its corresponding visited input element." aria-label="Dependency definition: An output element depends only on its corresponding visited input element.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.




## Behavior

`to-tuple` materializes the input array as a tuple without changing its elements. Null values and nested arrays, records, and tuples are preserved without recursive conversion.



## Examples

{% raw %}
```expressif
{1, "A", #true} | to-tuple → T(1, "A", #true)
```
{% endraw %}


**Kind:** Function  
**Scope:** `array`  
**Aliases:** None
{: .member-reference }
