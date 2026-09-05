---
layout: docs
title: "zip-cycle"
parent: "Combination functions"
grand_parent: "Array functions"
nav_order: 20
has_toc: false
permalink: /functions/array/combination/zip-cycle/
tags:
  - functions
  - array/combination
generated: true
---

```
array →
zip-cycle(
    array: array
) → array
```

Combines values from two arrays into two-element tuples until the longer array is exhausted, cycling each non-empty shorter array from its beginning. Returns an empty array when either input is empty and `null` when either value cannot be evaluated as an array.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `array` | `array` | Yes | Specifies the second array whose values form the second element of each tuple. |





## Behavior

Both inputs are materialized once. When both arrays are non-empty, the result has the cardinality of the longer array and indexes each input cyclically. If either array is empty, the result is empty.



## Examples

{% raw %}
```expressif
{1, 2, 3, 4} | zip-cycle({"a", "b"}) → {T(1, "a"), T(2, "b"), T(3, "a"), T(4, "b")}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/combination`  
**Aliases:** None
{: .member-reference }
