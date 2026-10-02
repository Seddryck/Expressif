---
layout: docs
title: "Aggregation functions"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 10
has_children: true
has_toc: false
permalink: /functions/array/aggregation/
tags:
  - functions
  - array
  - aggregation
generated: true
---

Reference documentation for Expressif functions in the `array/aggregation` scope.

| Name | Overview |
|:-----|:---------|
| [`any`]({{ '/functions/array/aggregation/any/' | relative_url }}) | Returns `true` when at least one accumulated boolean value is `true`. |
| [`broadcast`]({{ '/functions/array/aggregation/broadcast/' | relative_url }}) | Executes an accumulator once over the full input enumerable, then returns the final accumulated value repeated once for each input element. Returns `null` when the input is not an enumerable or is a string. |
| [`closest`]({{ '/functions/array/aggregation/closest/' | relative_url }}) | Returns the first non-null input value with the smallest absolute distance to the target. |
| [`common-prefix`]({{ '/functions/array/aggregation/common-prefix/' | relative_url }}) | Returns the longest prefix shared by all accumulated strings. |
| [`common-suffix`]({{ '/functions/array/aggregation/common-suffix/' | relative_url }}) | Returns the longest suffix shared by all accumulated strings. |
| [`concat`]({{ '/functions/array/aggregation/concat/' | relative_url }}) | Combines accumulated text values in source order, inserting the separator only between values. |
| [`count`]({{ '/functions/array/aggregation/count/' | relative_url }}) | Counts the number of accumulated items, including `null` values. |
| [`every`]({{ '/functions/array/aggregation/every/' | relative_url }}) | Returns `true` only when every accumulated boolean value is `true`. |
| [`first`]({{ '/functions/array/aggregation/first/' | relative_url }}) | Stores the first accumulated item and ignores all subsequent items. |
| [`fold`]({{ '/functions/array/aggregation/fold/' | relative_url }}) | Executes an accumulator once over the full input enumerable and returns the final accumulated value. Returns `null` when the input is not an enumerable or is a string. |
| [`last`]({{ '/functions/array/aggregation/last/' | relative_url }}) | Stores the most recently accumulated item. |
| [`max`]({{ '/functions/array/aggregation/max/' | relative_url }}) | Tracks the greatest numeric value found during accumulation. |
| [`min`]({{ '/functions/array/aggregation/min/' | relative_url }}) | Tracks the smallest numeric value found during accumulation. |
| [`only`]({{ '/functions/array/aggregation/only/' | relative_url }}) | Forwards only items satisfying the predicate to the wrapped accumulator. |
| [`reduce`]({{ '/functions/array/aggregation/reduce/' | relative_url }}) | Combines array elements in source order by repeatedly evaluating an expression against the accumulated value and current element. |
| [`scan`]({{ '/functions/array/aggregation/scan/' | relative_url }}) | Executes an accumulator progressively over the input enumerable and returns the intermediate accumulated value after each input element. Preserves input cardinality (one output item per input item). This differs from fold (final value only) and broadcast (final value repeated). Returns `null` when the input is not an enumerable or is a string. |
| [`sum`]({{ '/functions/array/aggregation/sum/' | relative_url }}) | Computes the sum of all accumulated numeric values. |
