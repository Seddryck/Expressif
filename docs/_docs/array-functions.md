---
title: Array functions
subtitle: Functions applicable to arrays
tags: [functions, array]
keywords: [broadcast, first-elements, fold, lag, last-elements, lead, map, reverse, scan, skip-first-elements, skip-last-elements, slice-elements] # AUTO-GENERATED KEYWORDS
---
<!-- START AUTO-GENERATED -->
##### broadcast

###### Alias: `array-to-broadcast`

###### Overview

Executes an accumulator once over the full input enumerable, then returns the final accumulated value repeated once for each input element. Returns `null` when the input is not an enumerable or is a string.

###### Parameter
* accumulator: Factory that creates the accumulator instance used for the broadcast execution.

##### first-elements

###### Alias: `first`

###### Overview



##### fold

###### Alias: `array-to-fold`

###### Overview

Executes an accumulator once over the full input enumerable and returns the final accumulated value. Returns `null` when the input is not an enumerable or is a string.

###### Parameter
* accumulator: Factory that creates the accumulator instance used for the fold execution.

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



<!-- END AUTO-GENERATED -->
