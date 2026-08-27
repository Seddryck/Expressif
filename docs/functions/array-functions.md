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
| [`adjacent`]({{ '/functions/array/adjacent/' | relative_url }}) | Evaluates an operation against every consecutive pair of input values. Returns `null` when the input cannot be evaluated. |
| [`array`]({{ '/functions/array/array/' | relative_url }}) | Constructs a new array by evaluating zero or more positional expressions from left to right against the same input. Spread arguments expand array values in place. This is the runtime-expression counterpart of array literal syntax. |
| [`broadcast`]({{ '/functions/array/broadcast/' | relative_url }}) | Executes an accumulator once over the full input enumerable, then returns the final accumulated value repeated once for each input element. Returns `null` when the input is not an enumerable or is a string. |
| [`chunk`]({{ '/functions/array/chunk/' | relative_url }}) | Splits an array into consecutive, non-overlapping chunks of at most the specified size, preserving a final partial chunk. It resembles a count-based tumbling window but, unlike general sliding or hopping windows, has no separate step and always keeps the final partial chunk. It does not group items by inactivity or time. Returns `null` when the input cannot be evaluated. |
| [`complement`]({{ '/functions/array/set/complement/' | relative_url }}) | Returns the distinct values from the specified array that do not appear in the pipeline input, preserving the specified array order. Returns `null` when the input cannot be evaluated. |
| [`difference`]({{ '/functions/array/set/difference/' | relative_url }}) | Returns the distinct values from the pipeline input that do not appear in the specified array, preserving the pipeline input order. Returns `null` when the input cannot be evaluated. |
| [`distinct`]({{ '/functions/array/set/distinct/' | relative_url }}) | Returns the unique values from the input array in the order of their first occurrence. Returns `null` when the input cannot be evaluated. |
| [`filter`]({{ '/functions/array/filter/' | relative_url }}) | Applies a predicate expression to each input item and returns only items for which the predicate evaluates to `true`. Returns `null` when the input is not an enumerable or is a string. |
| [`first-elements`]({{ '/functions/array/first-elements/' | relative_url }}) | Returns up to the requested number of elements from the start of the input enumerable. Returns `null` when the input is not an enumerable, is a string, or the count is negative. |
| [`fold`]({{ '/functions/array/fold/' | relative_url }}) | Executes an accumulator once over the full input enumerable and returns the final accumulated value. Returns `null` when the input is not an enumerable or is a string. |
| [`intersection`]({{ '/functions/array/set/intersection/' | relative_url }}) | Returns the distinct values found in both the pipeline input and the specified array, preserving the pipeline input order. Returns `null` when the input cannot be evaluated. |
| [`lag`]({{ '/functions/array/lag/' | relative_url }}) | Returns the previous value for each input element. The first output value is `null` because there is no previous element. Preserves input cardinality (one output item per input item). Returns `null` when the input is not an enumerable or is a string. |
| [`last-elements`]({{ '/functions/array/last-elements/' | relative_url }}) | Returns up to the requested number of elements from the end of the input enumerable, preserving their order. Returns `null` when the input is not an enumerable, is a string, or the count is negative. |
| [`lead`]({{ '/functions/array/lead/' | relative_url }}) | Returns the next value for each input element. The last output value is `null` because there is no next element. Preserves input cardinality (one output item per input item). Returns `null` when the input is not an enumerable or is a string. |
| [`map`]({{ '/functions/array/map/' | relative_url }}) | Applies a transformation expression to each input item and returns the transformed values. Preserves input cardinality (one output item per input item). Returns `null` when the input is not an enumerable or is a string. |
| [`pairwise`]({{ '/functions/array/pairwise/' | relative_url }}) | Returns each consecutive pair of input values as a tuple. Returns `null` when the input cannot be evaluated. |
| [`reverse`]({{ '/functions/array/reverse/' | relative_url }}) | Returns the input enumerable with elements emitted in the opposite order. Preserves input cardinality (one output item per input item). Returns `null` when the input is not an enumerable or is a string. |
| [`scan`]({{ '/functions/array/scan/' | relative_url }}) | Executes an accumulator progressively over the input enumerable and returns the intermediate accumulated value after each input element. Preserves input cardinality (one output item per input item). This differs from fold (final value only) and broadcast (final value repeated). Returns `null` when the input is not an enumerable or is a string. |
| [`skip-first-elements`]({{ '/functions/array/skip-first-elements/' | relative_url }}) | Omits the requested number of elements from the start of the input enumerable and returns the remainder. Returns `null` when the input is not an enumerable, is a string, or the count is negative. |
| [`skip-last-elements`]({{ '/functions/array/skip-last-elements/' | relative_url }}) | Omits the requested number of elements from the end of the input enumerable and returns the remainder. Returns `null` when the input is not an enumerable, is a string, or the count is negative. |
| [`slice-elements`]({{ '/functions/array/slice-elements/' | relative_url }}) | Returns the elements in the zero-based half-open range from start, inclusive, to end, exclusive. Returns `null` when the input is not an enumerable, is a string, or either bound is negative. |
| [`symmetric-difference`]({{ '/functions/array/set/symmetric-difference/' | relative_url }}) | Returns the distinct values that appear in exactly one of the two arrays, listing pipeline-input exclusives first and parameter-array exclusives second while preserving order within each source. Returns `null` when the input cannot be evaluated. |
| [`union`]({{ '/functions/array/set/union/' | relative_url }}) | Returns the distinct values appearing in either the pipeline input or the specified array, listing pipeline-input values first and argument-only values second while preserving order within each source. Returns `null` when the input cannot be evaluated. |
