---
layout: docs
title: "pairwise"
parent: "Sequencing functions"
grand_parent: "Array functions"
nav_order: 40
has_toc: false
permalink: /functions/array/sequencing/pairwise/
tags:
  - functions
  - array/sequencing
generated: true
---

```
array →
pairwise() → array
```

Returns each consecutive pair of input values as a tuple. Returns `null` when the input cannot be evaluated.



## Parameters



This function has no parameters.



## Value shape

- **Pipeline input:** `array<T>`
- **Returns:** `array<tuple<T, T>>`
- **Nullability:** The result is nullable when the pipeline input is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- **Cardinality:** `non-increasing`
- **Dependency:** `prefix`
- **Ordering:** `preserved`

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.





## Examples

{% raw %}
```expressif
{1, 2, 3} | pairwise → {T(1, 2), T(2, 3)}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/sequencing`  
**Aliases:** `pairwise`
{: .member-reference }
