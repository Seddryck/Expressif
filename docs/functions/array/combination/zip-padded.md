---
layout: docs
title: "zip-padded"
parent: "Combination functions"
grand_parent: "Array functions"
nav_order: 70
has_toc: false
permalink: /functions/array/combination/zip-padded/
tags:
  - functions
  - array/combination
generated: true
---

```
array →
zip-padded(
    array: array
) → array
```

Combines corresponding values from the input array and a second array into two-element tuples until both arrays are exhausted, using `null` for a missing value. Returns `null` when either value cannot be evaluated as an array.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `array` | `array` | Yes | Specifies the second array whose values form the second element of each tuple. |



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
{1, 2, 3} | zip-padded({"a", "b"}) → {T(1, "a"), T(2, "b"), T(3, #null)}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/combination`  
**Aliases:** None
{: .member-reference }
