---
layout: docs
title: "distinct"
parent: "Set functions"
grand_parent: "Array functions"
nav_order: 30
has_toc: false
permalink: /functions/array/set/distinct/
tags:
  - functions
  - array/set
generated: true
---

```
array →
distinct() → array
```

Returns the unique values from the input array in the order of their first occurrence. Returns `null` when the input cannot be evaluated.



## Parameters



This function has no parameters.



## Value shape

- **Pipeline input:** `array<T>`
- **Returns:** `array<T>`
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
{1, 2, 3} | distinct → {1, 2, 3}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/set`  
**Aliases:** `distinct`
{: .member-reference }
