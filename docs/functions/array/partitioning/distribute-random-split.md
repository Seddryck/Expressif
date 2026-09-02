---
layout: docs
title: "distribute-random-split"
parent: "Partitioning functions"
grand_parent: "Array functions"
nav_order: 60
has_toc: false
permalink: /functions/array/partitioning/distribute-random-split/
tags:
  - functions
  - array/partitioning
generated: true
---

```
array →
distribute-random-split(
    weights: array,
    seed?: integer
) → array
```

Randomly distributes array values among output arrays according to relative output weights. Returns `null` when the input, weights, or seed cannot be evaluated.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `weights` | `array` | Yes | Specifies a non-empty array of finite, non-negative output weights with a positive total. |
| `seed` | `integer` | No | Specifies an optional seed that makes assignments reproducible on the same runtime version. |





## Behavior

The weights are normalized by their total and specify assignment probabilities rather than exact output cardinalities. Each input value is independently assigned to exactly one output array. The result contains one array per weight, including empty arrays, and preserves relative input order within each output. Reusing the same seed, input, and weights produces the same result on the same runtime version. Empty input still validates the weights and returns one empty array per valid weight.



## Examples

{% raw %}
```expressif
{1, 2, 3, 4, 5} | distribute-random-split({1, 0}, 42) → {{1, 2, 3, 4, 5}, {}}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/partitioning`  
**Aliases:** None
{: .member-reference }
