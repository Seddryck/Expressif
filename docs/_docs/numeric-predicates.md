---
title: Numeric predicates
subtitle: Predicates applicable to numeric values
tags: [predicates, numeric]
keywords: [equal-to, even, greater-than, greater-than-or-equal, integer, less-than, less-than-or-equal, modulo, negative, negative-or-zero, odd, one, opposite, positive, positive-or-zero, within-interval, zero, zero-or-null] # AUTO-GENERATED KEYWORDS
---
<!-- START AUTO-GENERATED -->
##### equal-to

###### Alias: `numeric-is-equal-to`

###### Overview

Returns true if the numeric value passed as argument is equal to the numeric value passed as parameter.

###### Parameter
* reference: A numeric value to compare to the argument.

##### even

###### Alias: `numeric-is-even`

###### Overview

Returns `true` if the numeric value passed as argument is even. Returns `false` otherwise.

##### greater-than

###### Alias: `numeric-is-greater-than`

###### Overview

Returns true if the numeric value passed as argument is greater than the numeric value passed as parameter. Returns `false` otherwise.

###### Parameter
* reference: A numeric value to compare to the argument.

##### greater-than-or-equal

###### Alias: `numeric-is-greater-than-or-equal`

###### Overview

Returns true if the numeric value passed as argument is greater than or equal to the numeric value passed as parameter. Returns `false` otherwise.

###### Parameter
* reference: A numeric value to compare to the argument.

##### integer

###### Alias: `numeric-is-integer`

###### Overview

Returns true if the numeric value passed as argument is an integer value. Returns `false` otherwise.

##### less-than

###### Alias: `numeric-is-less-than`

###### Overview

Returns true if the numeric value passed as argument is less than the numeric value passed as parameter. Returns `false` otherwise.

###### Parameter
* reference: A numeric value to compare to the argument.

##### less-than-or-equal

###### Alias: `numeric-is-less-than-or-equal`

###### Overview

Returns true if the numeric value passed as argument is less than or equal to the numeric value passed as parameter. Returns `false` otherwise.

###### Parameter
* reference: A numeric value to compare to the argument.

##### modulo

###### Alias: `numeric-is-modulo`

###### Overview

Returns `true` if the division of the numeric value passed as argument by the modulus provided as parameter value is equal to the remainder provided as a second parameter. If no remainder is provided then 0 is expected. Returns `false` otherwise.

###### Parameters
* modulus: An integer value used as the modulus.
* remainder (optional) : An integer value defined as the expected reminder.

##### negative

###### Alias: `numeric-is-negative`

###### Overview

Returns true if the numeric argument is less than 0.

##### negative-or-zero

###### Alias: `numeric-is-negative-or-zero`

###### Overview

Returns true if the numeric argument is less or equal to 0.

##### odd

###### Alias: `numeric-is-odd`

###### Overview

Returns `true` if the numeric value passed as argument is odd. Returns `false` otherwise.

##### one

###### Alias: `numeric-is-one`

###### Overview

Returns true if the numeric argument is equal to 1.

##### opposite

###### Alias: `numeric-is-opposite`

###### Overview

Returns true if the numeric value passed as argument additive inverse of the numeric value passed as parameter. Returns `false` otherwise.

###### Parameter
* reference: A numeric value to compare to the argument.

##### positive

###### Alias: `numeric-is-positive`

###### Overview

Returns true if the numeric argument is greater than 0.

##### positive-or-zero

###### Alias: `numeric-is-positive-or-zero`

###### Overview

Returns true if the numeric argument is greater or equal to 0.

##### within-interval

###### Alias: `numeric-is-within-interval`

###### Overview

Returns true if the numeric value passed as argument is between the lower bound and the upper bound defined in the interval. Returns `false` otherwise.

###### Parameter
* interval: A numeric interval to compare to the argument.

##### zero

###### Alias: `numeric-is-zero`

###### Overview

Returns true if the numeric argument is equal to 0.

##### zero-or-null

###### Alias: `numeric-is-zero-or-null`

###### Overview

Returns true if the numeric value passed as argument is equal to `0` or `null`. Returns `false` otherwise.

<!-- END AUTO-GENERATED -->
