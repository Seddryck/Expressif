---
title: Numeric predicates
subtitle: Predicates applicable to numeric values
tags: [predicates, numeric]
keywords: [equal-to, even, greater-than, greater-than-or-equal, integer, less-than, less-than-or-equal, modulo, negative, negative-or-zero, odd, one, opposite, positive, positive-or-zero, within-interval, zero, zero-or-null] # AUTO-GENERATED KEYWORDS
---
<!-- START AUTO-GENERATED -->
##### is-equal-to
keywords: [is-equal-to, is-even, is-greater-than, is-greater-than-or-equal, is-integer, is-less-than, is-less-than-or-equal, is-modulo, is-negative, is-negative-or-zero, is-odd, is-one, is-opposite, is-positive, is-positive-or-zero, is-within-interval, is-zero, is-zero-or-null] # AUTO-GENERATED KEYWORDS
###### Aliases: `equal-to`, `numeric-is-equal-to`

###### Overview

Returns true if the numeric value passed as argument is equal to the numeric value passed as parameter.

###### Parameter
* reference: A numeric value to compare to the argument.

##### is-even

###### Aliases: `even`, `numeric-is-even`

###### Overview

Returns `true` if the numeric value passed as argument is even. Returns `false` otherwise.

##### is-greater-than

###### Aliases: `greater-than`, `numeric-is-greater-than`

###### Overview

Returns true if the numeric value passed as argument is greater than the numeric value passed as parameter. Returns `false` otherwise.

###### Parameter
* reference: A numeric value to compare to the argument.

##### is-greater-than-or-equal

###### Aliases: `greater-than-or-equal`, `numeric-is-greater-than-or-equal`

###### Overview

Returns true if the numeric value passed as argument is greater than or equal to the numeric value passed as parameter. Returns `false` otherwise.

###### Parameter
* reference: A numeric value to compare to the argument.

##### is-integer

###### Aliases: `integer`, `numeric-is-integer`

###### Overview

Returns true if the numeric value passed as argument is an integer value. Returns `false` otherwise.

##### is-less-than

###### Aliases: `less-than`, `numeric-is-less-than`

###### Overview

Returns true if the numeric value passed as argument is less than the numeric value passed as parameter. Returns `false` otherwise.

###### Parameter
* reference: A numeric value to compare to the argument.

##### is-less-than-or-equal

###### Aliases: `less-than-or-equal`, `numeric-is-less-than-or-equal`

###### Overview

Returns true if the numeric value passed as argument is less than or equal to the numeric value passed as parameter. Returns `false` otherwise.

###### Parameter
* reference: A numeric value to compare to the argument.

##### is-modulo

###### Aliases: `modulo`, `numeric-is-modulo`

###### Overview

Returns `true` if the division of the numeric value passed as argument by the modulus provided as parameter value is equal to the remainder provided as a second parameter. If no remainder is provided then 0 is expected. Returns `false` otherwise.

###### Parameters
* modulus: An integer value used as the modulus.
* remainder (optional) : An integer value defined as the expected reminder.

##### is-negative

###### Aliases: `negative`, `numeric-is-negative`

###### Overview

Returns true if the numeric argument is less than 0.

##### is-negative-or-zero

###### Aliases: `negative-or-zero`, `numeric-is-negative-or-zero`

###### Overview

Returns true if the numeric argument is less or equal to 0.

##### is-odd

###### Aliases: `odd`, `numeric-is-odd`

###### Overview

Returns `true` if the numeric value passed as argument is odd. Returns `false` otherwise.

##### is-one

###### Aliases: `one`, `numeric-is-one`

###### Overview

Returns true if the numeric argument is equal to 1.

##### is-opposite

###### Aliases: `opposite`, `numeric-is-opposite`

###### Overview

Returns true if the numeric value passed as argument additive inverse of the numeric value passed as parameter. Returns `false` otherwise.

###### Parameter
* reference: A numeric value to compare to the argument.

##### is-positive

###### Aliases: `positive`, `numeric-is-positive`

###### Overview

Returns true if the numeric argument is greater than 0.

##### is-positive-or-zero

###### Aliases: `positive-or-zero`, `numeric-is-positive-or-zero`

###### Overview

Returns true if the numeric argument is greater or equal to 0.

##### is-within-interval

###### Aliases: `within-interval`, `numeric-is-within-interval`

###### Overview

Returns true if the numeric value passed as argument is between the lower bound and the upper bound defined in the interval. Returns `false` otherwise.

###### Parameter
* interval: A numeric interval to compare to the argument.

##### is-zero

###### Aliases: `zero`, `numeric-is-zero`

###### Overview

Returns true if the numeric argument is equal to 0.

##### is-zero-or-null

###### Aliases: `zero-or-null`, `numeric-is-zero-or-null`

###### Overview

Returns true if the numeric value passed as argument is equal to `0` or `null`. Returns `false` otherwise.

<!-- END AUTO-GENERATED -->
