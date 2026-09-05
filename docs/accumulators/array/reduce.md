---
layout: docs
title: "reduce"
parent: "Array accumulators"
grand_parent: "Accumulators library"
nav_order: 80
has_toc: false
permalink: /accumulators/array/reduce/
tags:
  - accumulators
  - array
generated: true
---

```
array →
reduce(
    operation: expression,
    initial?: any
) → any
```

Combines array elements in source order by repeatedly evaluating an expression against the accumulated value and current element.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `operation` | `expression` | Yes | Specifies the expression evaluated against each accumulated-value/current-element tuple. |
| `initial` | `any` | No | Specifies the initial accumulated value and the result returned for an empty array. |





## Behavior

The combining expression receives a two-element tuple: `$0` is the accumulated value and `$1` is the current element. Without `initial`, the first element becomes the accumulated value and an empty array returns `null`. With `initial`, evaluation starts by combining it with the first element, and an empty array returns the initial value unchanged.



## Examples

{% raw %}
```expressif
{1, 2, 3, 4} | reduce(add($0, $1)) → 10
{1, 2, 3} | reduce(add($0, $1), 10) → 16
```
{% endraw %}


**Kind:** Accumulator  
**Scope:** `array`  
**Aliases:** `reduce`
{: .member-reference }
