---
title: Special functions
subtitle: Functions applicable to special values
tags: [functions]
keywords: [any-to-any, coalesce, neutral, null-to-value, tuple-at, tuple-first, tuple-second, value-to-value] # AUTO-GENERATED KEYWORDS
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
