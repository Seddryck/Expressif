---
layout: docs
title: "Combination functions"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 10
has_children: true
has_toc: false
permalink: /functions/array/combination/
tags:
  - functions
  - array
  - combination
generated: true
---

Reference documentation for Expressif functions in the `array/combination` scope.

| Name | Overview |
|:-----|:---------|
| [`join`]({{ '/functions/array/combination/join/' | relative_url }}) | Emits a pair for every matching left and right value, omitting left values without a match. Array right-hand values are grouped by key before lookup. |
| [`join-full`]({{ '/functions/array/combination/join-full/' | relative_url }}) | Emits every matching pair and preserves unmatched values from both sides with #null in the absent side. Array right-hand values are grouped by key before lookup. Null keys match null keys; composite keys use structural equality and duplicate values produce Cartesian combinations. Null or invalid collection inputs return null. |
| [`join-left`]({{ '/functions/array/combination/join-left/' | relative_url }}) | Emits every matching pair and preserves unmatched left values with #null in the absent side. Array right-hand values are grouped by key before lookup. Null keys match null keys; composite keys use structural equality and duplicate values produce Cartesian combinations. Null or invalid collection inputs return null. |
| [`join-right`]({{ '/functions/array/combination/join-right/' | relative_url }}) | Emits every matching pair and preserves unmatched right values with #null in the absent side. Array right-hand values are grouped by key before lookup. Null keys match null keys; composite keys use structural equality and duplicate values produce Cartesian combinations. Null or invalid collection inputs return null. |
| [`zip`]({{ '/functions/array/combination/zip/' | relative_url }}) | Combines corresponding values from the input array and a second array into two-element tuples, stopping when either array is exhausted. Returns `null` when either value cannot be evaluated as an array. |
| [`zip-cycle`]({{ '/functions/array/combination/zip-cycle/' | relative_url }}) | Combines values from two arrays into two-element tuples until the longer array is exhausted, cycling each non-empty shorter array from its beginning. Returns an empty array when both inputs are empty and `null` when exactly one input is empty or either value cannot be evaluated as an array. |
| [`zip-padded`]({{ '/functions/array/combination/zip-padded/' | relative_url }}) | Combines corresponding values from the input array and a second array into two-element tuples until both arrays are exhausted, using `null` for a missing value. Returns `null` when either value cannot be evaluated as an array. |
| [`zip-strict`]({{ '/functions/array/combination/zip-strict/' | relative_url }}) | Combines corresponding values from equally sized input and parameter arrays into two-element tuples. Returns `null` when the arrays have different lengths or either value cannot be evaluated as an array. |
