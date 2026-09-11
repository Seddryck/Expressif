---
layout: docs
title: "Array functions"
parent: "Functions library"

nav_order: 10
has_children: true
has_toc: false
permalink: /functions/array-functions/
tags:
  - functions
  - array

generated: true
---

Reference documentation for Expressif functions in the `array` scope.

| Name | Overview |
|:-----|:---------|
| [`closest-by`]({{ '/functions/array/selection/closest-by/' | relative_url }}) | Returns the original source element whose expression result is nearest to the numeric target. Preserves the first match on ties and skips null criteria. Returns `null` for empty arrays, all-null criteria, or non-array input. |
| [`first-elements`]({{ '/functions/array/selection/first-elements/' | relative_url }}) | Returns up to the requested number of elements from the start of the input enumerable. Returns `null` when the input is not an enumerable, is a string, or the count is negative. |
| [`last-elements`]({{ '/functions/array/selection/last-elements/' | relative_url }}) | Returns up to the requested number of elements from the end of the input enumerable, preserving their order. Returns `null` when the input is not an enumerable, is a string, or the count is negative. |
| [`flat-map`]({{ '/functions/array/flat-map/' | relative_url }}) | Evaluates an array-producing expression for each input element and concatenates the resulting arrays in order. Flattens one level, preserving nested arrays and null elements. Empty arrays contribute no elements. Throws an argument error when an expression result is not an array, including null or text. |
| [`map-over`]({{ '/functions/array/map-over/' | relative_url }}) | Evaluates an expression once for every supplied value while preserving the pipeline input as the expression input. Tuple values are expanded into positional arguments for a bare callable. Returns `null` when values is not enumerable or is text. |
| [`map-with`]({{ '/functions/array/map-with/' | relative_url }}) | Evaluates an expression once for every supplied value, using that value as the pipeline input and the outer input as its argument. Tuple values remain ordinary pipeline values and are not expanded. Returns `null` when values is not enumerable or is text. |
| [`max-by`]({{ '/functions/array/selection/max-by/' | relative_url }}) | Returns the original source element whose expression result is greatest. Preserves the first match on ties and skips null criteria. Returns `null` for empty arrays, all-null criteria, or non-array input. |
| [`min-by`]({{ '/functions/array/selection/min-by/' | relative_url }}) | Returns the original source element whose expression result is smallest. Preserves the first match on ties and skips null criteria. Returns `null` for empty arrays, all-null criteria, or non-array input. |
| [`single`]({{ '/functions/array/selection/single/' | relative_url }}) | Returns the only element of the input array without transforming it. Returns `null` when the input is empty, contains more than one element, or cannot be evaluated as an array. |
| [`skip-first-elements`]({{ '/functions/array/selection/skip-first-elements/' | relative_url }}) | Omits the requested number of elements from the start of the input enumerable and returns the remainder. Returns `null` when the input is not an enumerable, is a string, or the count is negative. |
| [`skip-last-elements`]({{ '/functions/array/selection/skip-last-elements/' | relative_url }}) | Omits the requested number of elements from the end of the input enumerable and returns the remainder. Returns `null` when the input is not an enumerable, is a string, or the count is negative. |
| [`slice-elements`]({{ '/functions/array/selection/slice-elements/' | relative_url }}) | Returns the elements in the zero-based half-open range from start, inclusive, to end, exclusive. Returns `null` when the input is not an enumerable, is a string, or either bound is negative. |
| [`value-at`]({{ '/functions/array/selection/value-at/' | relative_url }}) | Returns the input item at the specified zero-based position. Returns `null` when the position is negative or out of range, or the input cannot be evaluated. |
| [`zip-cycle`]({{ '/functions/array/combination/zip-cycle/' | relative_url }}) | Combines values from two arrays into two-element tuples until the longer array is exhausted, cycling each non-empty shorter array from its beginning. Returns an empty array when both inputs are empty and `null` when exactly one input is empty or either value cannot be evaluated as an array. |
| [`zip-padded`]({{ '/functions/array/combination/zip-padded/' | relative_url }}) | Combines corresponding values from the input array and a second array into two-element tuples until both arrays are exhausted, using `null` for a missing value. Returns `null` when either value cannot be evaluated as an array. |
| [`zip-strict`]({{ '/functions/array/combination/zip-strict/' | relative_url }}) | Combines corresponding values from equally sized input and parameter arrays into two-element tuples. Returns `null` when the arrays have different lengths or either value cannot be evaluated as an array. |
| [`zip`]({{ '/functions/array/combination/zip/' | relative_url }}) | Combines corresponding values from the input array and a second array into two-element tuples, stopping when either array is exhausted. Returns `null` when either value cannot be evaluated as an array. |
