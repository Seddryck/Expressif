---
layout: docs
title: "scan"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 140
has_toc: false
permalink: /functions/array/scan/
tags:
  - functions
  - array
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





## Examples

```expressif
{1, 2, 3} | scan(sum) → {1, 3, 6}
```


**Kind:** Function  
**Scope:** `array`  
**Aliases:** `array-to-scan`
{: .member-reference }
