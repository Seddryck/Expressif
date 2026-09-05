---
layout: docs
title: "Array accumulators"
parent: "Accumulators library"

nav_order: 10
has_children: true
has_toc: false
permalink: /accumulators/array-accumulators/
tags:
  - accumulators
  - array

generated: true
---

Reference documentation for Expressif accumulators in the `array` scope.

| Name | Overview |
|:-----|:---------|
| [`any`]({{ '/accumulators/array/any/' | relative_url }}) | Returns `true` when at least one accumulated boolean value is `true`. |
| [`count`]({{ '/accumulators/array/count/' | relative_url }}) | Counts the number of accumulated items, including `null` values. |
| [`every`]({{ '/accumulators/array/every/' | relative_url }}) | Returns `true` only when every accumulated boolean value is `true`. |
| [`first`]({{ '/accumulators/array/first/' | relative_url }}) | Stores the first accumulated item and ignores all subsequent items. |
| [`last`]({{ '/accumulators/array/last/' | relative_url }}) | Stores the most recently accumulated item. |
| [`max`]({{ '/accumulators/array/max/' | relative_url }}) | Tracks the greatest numeric value found during accumulation. |
| [`min`]({{ '/accumulators/array/min/' | relative_url }}) | Tracks the smallest numeric value found during accumulation. |
| [`reduce`]({{ '/accumulators/array/reduce/' | relative_url }}) | Combines array elements in source order by repeatedly evaluating an expression against the accumulated value and current element. |
| [`sum`]({{ '/accumulators/array/sum/' | relative_url }}) | Computes the sum of all accumulated numeric values. |
