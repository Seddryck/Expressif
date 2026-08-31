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
| [`adjacent`]({{ '/functions/array/sequencing/adjacent/' | relative_url }}) | Evaluates an operation against every consecutive pair of input values. Returns `null` when the input cannot be evaluated. |
| [`array`]({{ '/functions/array/array/' | relative_url }}) | Constructs a new array by evaluating zero or more positional expressions from left to right against the same input. Spread arguments expand array values in place. This is the runtime-expression counterpart of array literal syntax. |
| [`broadcast`]({{ '/functions/array/aggregation/broadcast/' | relative_url }}) | Executes an accumulator once over the full input enumerable, then returns the final accumulated value repeated once for each input element. Returns `null` when the input is not an enumerable or is a string. |
| [`cardinality`]({{ '/functions/array/cardinality/' | relative_url }}) | Returns the number of elements in the input array. |
| [`chunk`]({{ '/functions/array/partitioning/chunk/' | relative_url }}) | Splits an array into consecutive, non-overlapping chunks of at most the specified size, preserving a final partial chunk. It resembles a count-based tumbling window but, unlike general sliding or hopping windows, has no separate step and always keeps the final partial chunk. It does not group items by inactivity or time. Returns `null` when the input cannot be evaluated. |
| [`complement`]({{ '/functions/array/set/complement/' | relative_url }}) | Returns the distinct values from the specified array that do not appear in the pipeline input, preserving the specified array order. Returns `null` when the input cannot be evaluated. |
| [`difference`]({{ '/functions/array/set/difference/' | relative_url }}) | Returns the distinct values from the pipeline input that do not appear in the specified array, preserving the pipeline input order. Returns `null` when the input cannot be evaluated. |
| [`distinct`]({{ '/functions/array/set/distinct/' | relative_url }}) | Returns the unique values from the input array in the order of their first occurrence. Returns `null` when the input cannot be evaluated. |
| [`filter`]({{ '/functions/array/filter/' | relative_url }}) | Applies a predicate expression to each input item and returns only items for which the predicate evaluates to `true`. Returns `null` when the input is not an enumerable or is a string. |
| [`first-elements`]({{ '/functions/array/selection/first-elements/' | relative_url }}) | Returns up to the requested number of elements from the start of the input enumerable. Returns `null` when the input is not an enumerable, is a string, or the count is negative. |
| [`fold`]({{ '/functions/array/aggregation/fold/' | relative_url }}) | Executes an accumulator once over the full input enumerable and returns the final accumulated value. Returns `null` when the input is not an enumerable or is a string. |
| [`generate`]({{ '/functions/array/generate/' | relative_url }}) | Generates an array by repeatedly transforming a seed while a condition is satisfied. |
| [`intersection`]({{ '/functions/array/set/intersection/' | relative_url }}) | Returns the distinct values found in both the pipeline input and the specified array, preserving the pipeline input order. Returns `null` when the input cannot be evaluated. |
| [`lag`]({{ '/functions/array/sequencing/lag/' | relative_url }}) | Returns the previous value for each input element. The first output value is `null` because there is no previous element. Preserves input cardinality (one output item per input item). Returns `null` when the input is not an enumerable or is a string. |
| [`last-elements`]({{ '/functions/array/selection/last-elements/' | relative_url }}) | Returns up to the requested number of elements from the end of the input enumerable, preserving their order. Returns `null` when the input is not an enumerable, is a string, or the count is negative. |
| [`lead`]({{ '/functions/array/sequencing/lead/' | relative_url }}) | Returns the next value for each input element. The last output value is `null` because there is no next element. Preserves input cardinality (one output item per input item). Returns `null` when the input is not an enumerable or is a string. |
| [`map`]({{ '/functions/array/map/' | relative_url }}) | Applies a transformation expression to each input item and returns the transformed values. Preserves input cardinality (one output item per input item). Returns `null` when the input is not an enumerable or is a string. |
| [`pairwise`]({{ '/functions/array/sequencing/pairwise/' | relative_url }}) | Returns each consecutive pair of input values as a tuple. Returns `null` when the input cannot be evaluated. |
| [`position-of`]({{ '/functions/array/sequencing/position-of/' | relative_url }}) | Returns the zero-based position of the first input item equal to the specified value. Returns `null` when no item matches or the input cannot be evaluated. |
| [`reverse`]({{ '/functions/array/sequencing/reverse/' | relative_url }}) | Returns the input enumerable with elements emitted in the opposite order. Preserves input cardinality (one output item per input item). Returns `null` when the input is not an enumerable or is a string. |
| [`scan`]({{ '/functions/array/aggregation/scan/' | relative_url }}) | Executes an accumulator progressively over the input enumerable and returns the intermediate accumulated value after each input element. Preserves input cardinality (one output item per input item). This differs from fold (final value only) and broadcast (final value repeated). Returns `null` when the input is not an enumerable or is a string. |
| [`skip-first-elements`]({{ '/functions/array/selection/skip-first-elements/' | relative_url }}) | Omits the requested number of elements from the start of the input enumerable and returns the remainder. Returns `null` when the input is not an enumerable, is a string, or the count is negative. |
| [`skip-last-elements`]({{ '/functions/array/selection/skip-last-elements/' | relative_url }}) | Omits the requested number of elements from the end of the input enumerable and returns the remainder. Returns `null` when the input is not an enumerable, is a string, or the count is negative. |
| [`slice-elements`]({{ '/functions/array/selection/slice-elements/' | relative_url }}) | Returns the elements in the zero-based half-open range from start, inclusive, to end, exclusive. Returns `null` when the input is not an enumerable, is a string, or either bound is negative. |
| [`symmetric-difference`]({{ '/functions/array/set/symmetric-difference/' | relative_url }}) | Returns the distinct values that appear in exactly one of the two arrays, listing pipeline-input exclusives first and parameter-array exclusives second while preserving order within each source. Returns `null` when the input cannot be evaluated. |
| [`union`]({{ '/functions/array/set/union/' | relative_url }}) | Returns the distinct values appearing in either the pipeline input or the specified array, listing pipeline-input values first and argument-only values second while preserving order within each source. Returns `null` when the input cannot be evaluated. |
| [`value-at`]({{ '/functions/array/selection/value-at/' | relative_url }}) | Returns the input item at the specified zero-based position. Returns `null` when the position is negative or out of range, or the input cannot be evaluated. |
| [`with-position`]({{ '/functions/array/sequencing/with-position/' | relative_url }}) | Returns each input item paired with its zero-based position as a tuple in `(position, value)` order. Preserves input order and cardinality. Position terminology distinguishes sequence locations from indexes used to accelerate searches. Returns `null` when the input cannot be evaluated. |
| [`zip`]({{ '/functions/array/combination/zip/' | relative_url }}) | Combines corresponding values from the input array and a second array into two-element tuples, stopping when either array is exhausted. Returns `null` when either value cannot be evaluated as an array. |
| [`zip-padded`]({{ '/functions/array/combination/zip-padded/' | relative_url }}) | Combines corresponding values from the input array and a second array into two-element tuples until both arrays are exhausted, using `null` for a missing value. Returns `null` when either value cannot be evaluated as an array. |
| [`zip-strict`]({{ '/functions/array/combination/zip-strict/' | relative_url }}) | Combines corresponding values from equally sized input and parameter arrays into two-element tuples. Returns `null` when the arrays have different lengths or either value cannot be evaluated as an array. |
