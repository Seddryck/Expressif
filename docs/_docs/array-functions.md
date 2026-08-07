---
title: Array functions
subtitle: Functions applicable to arrays
tags: [functions, array]
keywords: [adjacent, broadcast, chunk, complement, difference, distinct, filter, first-elements, fold, intersection, lag, last-elements, lead, map, pairwise, reverse, scan, skip-first-elements, skip-last-elements, slice-elements, symmetric-difference, union] # AUTO-GENERATED KEYWORDS
---
<!-- START AUTO-GENERATED -->
##### adjacent

###### Alias: `adjacent`

###### Overview

Evaluates an operation against every consecutive pair of input values. Returns `null` when the input cannot be evaluated.

###### Parameter
* operation: Specifies the callable or open expression evaluated against each consecutive pair.

##### broadcast

###### Alias: `array-to-broadcast`

###### Overview

Executes an accumulator once over the full input enumerable, then returns the final accumulated value repeated once for each input element. Returns `null` when the input is not an enumerable or is a string.

###### Parameter
* accumulator: Factory that creates the accumulator instance used for the broadcast execution.

##### chunk

###### Alias: `chunk`

###### Overview

Splits an array into consecutive, non-overlapping chunks of at most the specified size, preserving a final partial chunk. It resembles a count-based tumbling window but, unlike general sliding or hopping windows, has no separate step and always keeps the final partial chunk. It does not group items by inactivity or time. Returns `null` when the input cannot be evaluated.

###### Parameter
* size: The strictly positive number of items in each chunk.

##### complement

###### Alias: `complement`

###### Overview

Returns the distinct values from the specified array that do not appear in the pipeline input, preserving the specified array order. Returns `null` when the input cannot be evaluated.

###### Parameter
* array: Specifies the reference array from which values present in the pipeline input are excluded.

##### difference

###### Alias: `difference`

###### Overview

Returns the distinct values from the pipeline input that do not appear in the specified array, preserving the pipeline input order. Returns `null` when the input cannot be evaluated.

###### Parameter
* array: Specifies the array containing values to exclude from the pipeline input.

##### distinct

###### Alias: `distinct`

###### Overview

Returns the unique values from the input array in the order of their first occurrence. Returns `null` when the input cannot be evaluated.

##### filter

###### Alias: `filter`

###### Overview

Applies a predicate expression to each input item and returns only items for which the predicate evaluates to `true`. Returns `null` when the input is not an enumerable or is a string.

###### Parameter
* predicate: Expression defining the predicate applied to each input item.

##### first-elements

###### Alias: `first`

###### Overview

Returns up to the requested number of elements from the start of the input enumerable. Returns `null` when the input is not an enumerable, is a string, or the count is negative.

###### Parameter
* count: Number of elements to return from the start of the input.

##### fold

###### Alias: `array-to-fold`

###### Overview

Executes an accumulator once over the full input enumerable and returns the final accumulated value. Returns `null` when the input is not an enumerable or is a string.

###### Parameter
* accumulator: Factory that creates the accumulator instance used for the fold execution.

##### intersection

###### Alias: `intersection`

###### Overview

Returns the distinct values found in both the pipeline input and the specified array, preserving the pipeline input order. Returns `null` when the input cannot be evaluated.

###### Parameter
* array: Specifies the array to compare with the pipeline input.

##### lag

###### Alias: `array-to-lag`

###### Overview

Returns the previous value for each input element. The first output value is `null` because there is no previous element. Preserves input cardinality (one output item per input item). Returns `null` when the input is not an enumerable or is a string.

##### last-elements

###### Alias: `last`

###### Overview

Returns up to the requested number of elements from the end of the input enumerable, preserving their order. Returns `null` when the input is not an enumerable, is a string, or the count is negative.

###### Parameter
* count: Number of elements to return from the end of the input.

##### lead

###### Alias: `array-to-lead`

###### Overview

Returns the next value for each input element. The last output value is `null` because there is no next element. Preserves input cardinality (one output item per input item). Returns `null` when the input is not an enumerable or is a string.

##### map

###### Alias: `map`

###### Overview

Applies a transformation expression to each input item and returns the transformed values. Preserves input cardinality (one output item per input item). Returns `null` when the input is not an enumerable or is a string.

###### Parameter
* transformation: Expression creating the transformation applied to each input item.

##### pairwise

###### Alias: `pairwise`

###### Overview

Returns each consecutive pair of input values as a tuple. Returns `null` when the input cannot be evaluated.

##### reverse

###### Alias: `reverse`

###### Overview

Returns the input enumerable with elements emitted in the opposite order. Preserves input cardinality (one output item per input item). Returns `null` when the input is not an enumerable or is a string.

##### scan

###### Alias: `array-to-scan`

###### Overview

Executes an accumulator progressively over the input enumerable and returns the intermediate accumulated value after each input element. Preserves input cardinality (one output item per input item). This differs from fold (final value only) and broadcast (final value repeated). Returns `null` when the input is not an enumerable or is a string.

###### Parameter
* accumulator: Factory that creates the accumulator instance used for the scan execution.

##### skip-first-elements

###### Alias: `skip-first`

###### Overview

Omits the requested number of elements from the start of the input enumerable and returns the remainder. Returns `null` when the input is not an enumerable, is a string, or the count is negative.

###### Parameter
* count: Number of elements to omit from the start of the input.

##### skip-last-elements

###### Alias: `skip-last`

###### Overview

Omits the requested number of elements from the end of the input enumerable and returns the remainder. Returns `null` when the input is not an enumerable, is a string, or the count is negative.

###### Parameter
* count: Number of elements to omit from the end of the input.

##### slice-elements

###### Alias: `slice`

###### Overview

Returns the elements in the zero-based half-open range from start, inclusive, to end, exclusive. Returns `null` when the input is not an enumerable, is a string, or either bound is negative.

###### Parameters
* start: Zero-based index of the first element to return.
* end: Zero-based exclusive index at which to stop returning elements.

##### symmetric-difference

###### Alias: `symmetric-difference`

###### Overview

Returns the distinct values that appear in exactly one of the two arrays, listing pipeline-input exclusives first and parameter-array exclusives second while preserving order within each source. Returns `null` when the input cannot be evaluated.

###### Parameter
* array: Specifies the second array to compare against the pipeline input.

##### union

###### Alias: `union`

###### Overview

Returns the distinct values appearing in either the pipeline input or the specified array, listing pipeline-input values first and argument-only values second while preserving order within each source. Returns `null` when the input cannot be evaluated.

###### Parameter
* array: Specifies the second array whose values are combined with the pipeline input.

<!-- END AUTO-GENERATED -->

## Map shorthand

Use `|> (...)` to apply a parenthesized expression to every item of an array. It is equivalent to `| map(...)`:

```text
{-1, 2, -3} |> (absolute | add(5)) | reverse
```

The parentheses are mandatory. The ordinary `|` after the closing parenthesis continues the pipeline with the complete mapped array, so the example applies `reverse` after mapping.
