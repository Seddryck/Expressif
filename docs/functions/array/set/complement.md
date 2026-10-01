---
layout: docs
title: "complement"
parent: "Set functions"
grand_parent: "Array functions"
nav_order: 10
has_toc: false
permalink: /functions/array/set/complement/
tags:
  - functions
  - array/set
generated: true
---

```
array →
complement(
    array: array
) → array
```

Returns the distinct values from the specified array that do not appear in the pipeline input, preserving the specified array order. Returns `null` when the input cannot be evaluated.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `array` | `array` | Yes | Specifies the reference array from which values present in the pipeline input are excluded. |



## Value shape

- **Pipeline input:** `array<T>`
- **Returns:** `array<U>`
- **`array`:** Returns `array<U>`.
- **Nullability:** The result is nullable when the pipeline input or the `array` parameter is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- **Cardinality:** `expanded`
- **Dependency:** `whole-input`
- **Ordering:** `preserved`

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

- **`array`:** Evaluated once in the enclosing context.



## Examples

{% raw %}
```expressif
{1, 2, 3} | complement({2, 3, 4}) → {4}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/set`  
**Aliases:** `complement`
{: .member-reference }
