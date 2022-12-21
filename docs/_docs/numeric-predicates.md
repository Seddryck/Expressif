---
title: Numeric predicates
subtitle: Predicates applicable to numeric values
tags: [predicates, numeric]
keywords: [equal-to, even, greater-than, greater-than-or-equal, integer, less-than, less-than-or-equal, modulo, odd, within-interval, zero-or-null] # AUTO-GENERATED KEYWORDS
---
<!-- START AUTO-GENERATED -->
##### equal-to
###### Overview

Returns true if the numeric value passed as argument is equal to the numeric value passed as parameter.

###### Parameter
* reference: A numeric value to compare to the argument

##### even
###### Overview

Returns `true` if the numeric value passed as argument is even. Returns `false` otherwise.

##### greater-than
###### Overview

Returns true if the numeric value passed as argument is greater than the numeric value passed as parameter. Returns `false` otherwise.

###### Parameter
* reference: A numeric value to compare to the argument

##### greater-than-or-equal
###### Overview

Returns true if the numeric value passed as argument is greater than or equal to the numeric value passed as parameter. Returns `false` otherwise.

###### Parameter
* reference: A numeric value to compare to the argument

##### integer
###### Overview

Returns true if the numeric value passed as argument is an integer value. Returns `false` otherwise.

##### less-than
###### Overview

Returns true if the numeric value passed as argument is less than the numeric value passed as parameter. Returns `false` otherwise.

###### Parameter
* reference: A numeric value to compare to the argument

##### less-than-or-equal
###### Overview

Returns true if the numeric value passed as argument is less than or equal to the numeric value passed as parameter. Returns `false` otherwise.

###### Parameter
* reference: A numeric value to compare to the argument

##### modulo
###### Overview

Returns `true` if the division of the numeric value passed as argument by the modulus provided as parameter value is equal to the remainder provided as a second parameter. If no remainder is provided then 0 is expected. Returns `false` otherwise.

###### Parameters
* modulus: An integer value used as the modulus.
* remainder (optional) : An integer value defined as the expected reminder.

##### odd
###### Overview

Returns `true` if the numeric value passed as argument is odd. Returns `false` otherwise.

##### within-interval
###### Overview

Returns true if the numeric value passed as argument is between the lower bound and the upper bound defined in the interval. Returns `false` otherwise.

###### Parameter
* interval: A numeric interval to compare to the argument

##### zero-or-null
###### Overview

Returns true if the numeric value passed as argument is equal to `0` or `null`. Returns `false` otherwise.

<!-- END AUTO-GENERATED -->
