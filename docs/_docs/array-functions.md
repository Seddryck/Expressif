---
title: Array functions
subtitle: Functions applicable to arrays
tags: [functions, array]
keywords: [broadcast, fold] # AUTO-GENERATED KEYWORDS
---
<!-- START AUTO-GENERATED -->
##### broadcast

###### Alias: `array-to-broadcast`

###### Overview

Executes an accumulator once over the full input enumerable, then returns
            the final accumulated value repeated once for each input element.
            Returns `null` when the input is not an enumerable or is a string.

###### Parameter
* accumulator: Factory that creates the accumulator instance used for the broadcast execution.

##### fold

###### Alias: `array-to-fold`

###### Overview

Executes an accumulator once over the full input enumerable and returns
            the final accumulated value.
            Returns `null` when the input is not an enumerable or is a string.

###### Parameter
* accumulator: Factory that creates the accumulator instance used for the fold execution.

<!-- END AUTO-GENERATED -->
