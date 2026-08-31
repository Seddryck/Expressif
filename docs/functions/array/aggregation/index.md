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
| [`broadcast`]({{ '/functions/array/aggregation/broadcast/' | relative_url }}) | Executes an accumulator once over the full input enumerable, then returns the final accumulated value repeated once for each input element. Returns `null` when the input is not an enumerable or is a string. |
| [`fold`]({{ '/functions/array/aggregation/fold/' | relative_url }}) | Executes an accumulator once over the full input enumerable and returns the final accumulated value. Returns `null` when the input is not an enumerable or is a string. |
| [`scan`]({{ '/functions/array/aggregation/scan/' | relative_url }}) | Executes an accumulator progressively over the input enumerable and returns the intermediate accumulated value after each input element. Preserves input cardinality (one output item per input item). This differs from fold (final value only) and broadcast (final value repeated). Returns `null` when the input is not an enumerable or is a string. |
