---
layout: docs
title: "distribute-condition"
parent: "Partitioning functions"
grand_parent: "Array functions"
nav_order: 50
has_toc: false
permalink: /functions/array/partitioning/distribute-condition/
tags:
  - functions
  - array/partitioning
generated: true
---

```
array →
distribute-condition(
    condition: predicate
) → array
```

Distributes array values into matching and non-matching groups by evaluating a predicate once for each value. Returns `null` when the input cannot be evaluated.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `condition` | `predicate` | Yes | Specifies the predicate used to classify each input value. |





## Behavior

`distribute-condition` returns exactly two arrays. The first contains values for which `condition` evaluates to `true`; the second contains values for which it evaluates to `false`. Each value occurs in exactly one output array, and relative input order is preserved within both arrays. Empty input returns two empty arrays.



## Examples

{% raw %}
```expressif
{1, 2, 3, 4, 5} | distribute-condition(is-even) → {{2, 4}, {1, 3, 5}}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/partitioning`  
**Aliases:** None
{: .member-reference }
