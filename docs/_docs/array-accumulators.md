---
title: Array accumulators
subtitle: Accumulators applicable to arrays
tags: [accumulators, array]
keywords: [any, count, every, first, last, max, min, sum] # AUTO-GENERATED KEYWORDS
---

`every` and `any` accumulate boolean values directly and take no predicate parameter. They are distinct from the `all(...)` and `none(...)` array predicates, which evaluate a supplied predicate against array elements.

<!-- START AUTO-GENERATED -->
##### any

###### Alias: `any`

###### Overview

Returns `true` when at least one accumulated boolean value is `true`. Returns `null` when an input cannot be evaluated.

##### count

###### Alias: `count`

###### Overview

Counts the number of accumulated items, including `null` values.

##### every

###### Alias: `every`

###### Overview

Returns `true` only when every accumulated boolean value is `true`. Returns `null` when an input cannot be evaluated.

##### first

###### Alias: `first`

###### Overview

Stores the first accumulated item and ignores all subsequent items.

##### last

###### Alias: `last`

###### Overview

Stores the most recently accumulated item.

##### max

###### Alias: `max`

###### Overview

Tracks the greatest numeric value found during accumulation.

##### min

###### Alias: `min`

###### Overview

Tracks the smallest numeric value found during accumulation.

##### sum

###### Alias: `sum`

###### Overview

Computes the sum of all accumulated numeric values.

<!-- END AUTO-GENERATED -->
