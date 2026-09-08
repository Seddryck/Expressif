---
layout: docs
title: "broadcast"
parent: "Aggregation functions"
grand_parent: "Array functions"
nav_order: 10
has_toc: false
permalink: /functions/array/aggregation/broadcast/
tags:
  - functions
  - array/aggregation
generated: true
---

```
array →
broadcast(
    accumulator: accumulator
) → array
```

Executes an accumulator once over the full input enumerable, then returns the final accumulated value repeated once for each input element. Returns `null` when the input is not an enumerable or is a string.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `accumulator` | `accumulator` | Yes | Factory that creates the accumulator instance used for the broadcast execution. |

## Argument evaluation

Visits each element of the array entering this call.

- **`accumulator`:** The selected accumulator receives each incoming array element through its accumulation lifecycle; the accumulator factory is not recreated for each element.

## Examples

{% raw %}
```expressif
{1, 2, 3} | broadcast(sum) → {6, 6, 6}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/aggregation`  
**Aliases:** `array-to-broadcast`
{: .member-reference }
