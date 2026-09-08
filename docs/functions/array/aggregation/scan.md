---
layout: docs
title: "scan"
parent: "Aggregation functions"
grand_parent: "Array functions"
nav_order: 30
has_toc: false
permalink: /functions/array/aggregation/scan/
tags:
  - functions
  - array/aggregation
generated: true
---

```
array →
scan(
    accumulator: accumulator
) → array
```

Executes an accumulator progressively over the input enumerable and returns the intermediate accumulated value after each input element. Preserves input cardinality (one output item per input item). This differs from fold (final value only) and broadcast (final value repeated). Returns `null` when the input is not an enumerable or is a string.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `accumulator` | `accumulator` | Yes | Factory that creates the accumulator instance used for the scan execution. |

## Argument evaluation

Visits each element of the array entering this call.

- **`accumulator`:** The selected accumulator receives each incoming array element through its accumulation lifecycle; the accumulator factory is not recreated for each element.

## Examples

{% raw %}
```expressif
{1, 2, 3} | scan(sum) → {1, 3, 6}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/aggregation`  
**Aliases:** `array-to-scan`
{: .member-reference }
