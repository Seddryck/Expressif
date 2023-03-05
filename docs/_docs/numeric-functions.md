---
title: Numeric functions
subtitle: Functions applicable to numeric values
tags: [functions, numeric]
keywords: [absolute, add, ceiling, clip, cube-power, cube-root, decrement, divide, floor, increment, integer, invert, multiply, nth-root, null-to-zero, oppose, power, round, sign, square-power, square-root, subtract] # AUTO-GENERATED KEYWORDS
---

For all numeric functions, unless otherwise specified, if the argument value is `null`, `empty` or `whitespace`, the function returns `null`.

<!-- START AUTO-GENERATED -->
##### absolute
###### Overview

Returns the absolute value of the argument value.

##### add
###### Overview

Returns the sum of an argument number and the parameter value.

###### Parameters
* value: The value to be added to the argument value.
* times: An integer between 0 and +Infinity, indicating the number of times to repeat the sum.

##### ceiling
###### Overview

Returns the smallest integer greater than or equal to the argument number.

##### clip
###### Overview

Returns the value of an argument number, unless it is smaller than min, in which case it returns min, or greater than max, in which case it returns max.

###### Parameters
* min: value returned in case the argument value is smaller than it.
* max: value returned in case the argument value is greater than it.

##### cube-power
###### Overview

Returns the the numeric argument value raised to the cube power.

##### cube-root
###### Overview

Returns cube root of the numeric argument value.

##### decrement
###### Overview

Returns the argument number decremented of one unit.

##### divide
###### Overview

Returns the argument number divided by the parameter value. If the parameter value is `0`, it returns `null`.

###### Parameter
* value: The value to divide the argument value.

##### floor
###### Overview

Returns the largest integer less than or equal to the argument number.

##### increment
###### Overview

Returns the argument number incremented of one unit.

##### integer
###### Overview

Returns the value of an argument number rounded to the nearest integer.

##### invert
###### Overview

Returns the reciprocal of the argument number, meaning the result of the division of 1 by the argument number. If the argument value is `0`, it returns `null`.

##### multiply
###### Overview

Returns the argument number multiplied by the parameter value.

###### Parameter
* value: The value to be multiplied by the argument value.

##### nth-root
###### Overview

Returns the root specified by the parameter value of the numeric argument value.

##### null-to-zero
###### Overview

Returns the unmodified argument value except if the argument value is `null`, `empty` or `whitespace` then it returns `0`.

##### oppose
###### Overview

Returns the integer being the additive inverse of the argument meaning that their sum is equal to zero. The opposite of 0 is 0.

##### power
###### Overview

Returns the the numeric argument value raised to the power specified by the parameter value.

##### round
###### Overview

Returns the value of an argument number to the specified number of fractional digits.

###### Parameter
* digits: An integer between 0 and +Infinity, indicating the number of fractional digits in the return value.

##### sign
###### Overview

Returns an integer that indicates the sign of the argument value. It returns -1 if the value is strictly negative, 0 if the value is 0 and 1 if the value is strictly positive.

##### square-power
###### Overview

Returns the the numeric argument value raised to the square power.

##### square-root
###### Overview

Returns square root of the numeric argument value.

##### subtract
###### Overview

Returns the difference between the argument number and the parameter value.

###### Parameters
* value: The value to be subtracted to the argument value.
* times: An integer between 0 and +Infinity, indicating the number of times to repeat the subtraction.

<!-- END AUTO-GENERATED -->
