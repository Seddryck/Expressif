---
title: Array functions
subtitle: Functions applicable to arrays
tags: [functions, array]
keywords: [broadcast, complement, difference, filter, first-elements, fold, intersection, lag, last-elements, lead, map, reverse, scan, skip-first-elements, skip-last-elements, slice-elements, symmetric-difference, union] # AUTO-GENERATED KEYWORDS
---
<!-- START AUTO-GENERATED -->
##### broadcast

###### Alias: `array-to-broadcast`

###### Overview

Executes an accumulator once over the full input enumerable, then returns the final accumulated value repeated once for each input element. Returns `null` when the input is not an enumerable or is a string.

###### Parameter
* accumulator: Factory that creates the accumulator instance used for the broadcast execution.

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

##### filter

###### Alias: `filter`

###### Overview

Applies a predicate expression to each input item and returns only items for which the predicate evaluates to `true`. Returns `null` when the input is not an enumerable or is a string.

##### first-elements

###### Alias: `first`

###### Overview



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



##### lead

###### Alias: `array-to-lead`

###### Overview

Returns the next value for each input element. The last output value is `null` because there is no next element. Preserves input cardinality (one output item per input item). Returns `null` when the input is not an enumerable or is a string.

##### map

###### Alias: `map`

###### Overview

Applies a transformation expression to each input item and returns the transformed values. Preserves input cardinality (one output item per input item). Returns `null` when the input is not an enumerable or is a string.

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



##### skip-last-elements

###### Alias: `skip-last`

###### Overview



##### slice-elements

###### Alias: `slice`

###### Overview



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
