---
title: Special functions
subtitle: Functions applicable to special values
tags: [functions]
keywords: [any-to-any, coalesce, coerce-boolean, coerce-date, coerce-datetime, coerce-int, coerce-numeric, coerce-text, coerce-time, neutral, null-to-value, tuple-at, tuple-first, tuple-second, value-to-value] # AUTO-GENERATED KEYWORDS
---
<!-- START AUTO-GENERATED -->
##### any-to-any
###### Overview

Returns `any`.

##### coalesce
###### Overview

Returns the first non-null result from two or more expressions evaluated from left to right against the same input. Returns `null` when every expression evaluates to `null`.

###### Parameter
* expressions: The candidate expressions, with at least two required.

##### coerce-boolean

###### Alias: `special-to-coerce-boolean`

###### Overview

Attempts to convert the input to a boolean value. Returns `null` when the input cannot be converted.

##### coerce-date

###### Alias: `special-to-coerce-date`

###### Overview

Attempts to convert the input to a date value. Returns `null` when the input cannot be converted.

##### coerce-datetime

###### Alias: `special-to-coerce-datetime`

###### Overview

Attempts to convert the input to a date-time value. Returns `null` when the input cannot be converted.

##### coerce-int

###### Alias: `special-to-coerce-int`

###### Overview

Attempts to convert the input to an integer value. Returns `null` when the input cannot be converted without loss.

##### coerce-numeric

###### Alias: `special-to-coerce-numeric`

###### Overview

Attempts to convert the input to a numeric value. Returns `null` when the input cannot be converted.

##### coerce-text

###### Alias: `special-to-coerce-text`

###### Overview

Attempts to convert the input to a text value. Returns `null` when the input cannot be converted.

##### coerce-time

###### Alias: `special-to-coerce-time`

###### Overview

Attempts to convert the input to a time value. Returns `null` when the input cannot be converted.

##### neutral

###### Alias: `Special-to-neutral`

###### Overview

Returns the argument value.

##### null-to-value
###### Overview

Returns the value passed as argument, except if the value is `null` then it returns `value`.

##### tuple-at

###### Alias: `tuple-at`

###### Overview

Returns the tuple field at the specified zero-based position. Returns `null` when the input is not a tuple or the position is out of range.

###### Parameter
* position: Specifies the zero-based position of the tuple field to return.

##### tuple-first

###### Alias: `tuple-first`

###### Overview

Returns the first field of a tuple. Returns `null` when the input is not a tuple.

##### tuple-second

###### Alias: `tuple-second`

###### Overview

Returns the second field of a tuple. Returns `null` when the input is not a tuple.

##### value-to-value
###### Overview

Returns `value` except if the argument value is `null` then it returns `null`.

<!-- END AUTO-GENERATED -->
