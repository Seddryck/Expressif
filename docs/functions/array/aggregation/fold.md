---
layout: docs
title: "fold"
parent: "Aggregation functions"
grand_parent: "Array functions"
nav_order: 20
has_toc: false
permalink: /functions/array/aggregation/fold/
tags:
  - functions
  - array/aggregation
generated: true
---

```
array →
fold(
    accumulator: accumulator
) → any
```

Executes an accumulator once over the full input enumerable and returns the final accumulated value. Returns `null` when the input is not an enumerable or is a string.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `accumulator` | `accumulator` | Yes | Factory that creates the accumulator instance used for the fold execution. |






## Examples

{% raw %}
```expressif
{1, 2, 3} | fold(sum) → 6
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/aggregation`  
**Aliases:** `array-to-fold`
{: .member-reference }
