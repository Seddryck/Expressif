---
title: Boolean predicates
subtitle: Predicates applicable to boolean values
tags: [predicates, boolean]
keywords: [and, false, false-or-null, identical-to, not, or, true, true-or-null, xor] # AUTO-GENERATED KEYWORDS
---
<!-- START AUTO-GENERATED -->
##### and

###### Alias: `boolean-is-and`

###### Overview

Returns the logical conjunction of the Boolean-converted input and a secondary predicate expression. Null-like and unconvertible values convert to `false`; zero converts to `false`; nonzero numbers and recognized true text convert to `true`. Evaluates the secondary expression only when the converted input is `true`.

###### Parameter
* expression: Specifies the secondary predicate expression evaluated when the converted input is `true`.

##### false

###### Alias: `boolean-is-false`

###### Overview

Returns `true` if the argument is effectively `false` else return `false`.

##### false-or-null

###### Alias: `boolean-is-false-or-null`

###### Overview

Returns `true` if the argument is effectively `false` or `null` else return `false`.

##### identical-to

###### Alias: `boolean-is-identical-to`

###### Overview

Returns `true` if the boolean passed as argument has the same value than the boolean passed as parameter. Returns `false` otherwise.

###### Parameter
* reference: A boolean value to compare to the argument.

##### not

###### Alias: `boolean-is-not`

###### Overview

Returns the logical negation of the Boolean-converted input. Null-like and unconvertible values convert to `false`; zero converts to `false`; nonzero numbers and recognized true text convert to `true`.

##### or

###### Alias: `boolean-is-or`

###### Overview

Returns the logical disjunction of the Boolean-converted input and a secondary predicate expression. Null-like and unconvertible values convert to `false`; zero converts to `false`; nonzero numbers and recognized true text convert to `true`. Evaluates the secondary expression only when the converted input is `false`.

###### Parameter
* expression: Specifies the secondary predicate expression evaluated when the converted input is `false`.

##### true

###### Alias: `boolean-is-true`

###### Overview

Returns `true` if the argument is effectively `true` else return `false`.

##### true-or-null

###### Alias: `boolean-is-true-or-null`

###### Overview

Returns `true` if the argument is effectively `true` or `null` else return `false`.

##### xor

###### Alias: `boolean-is-xor`

###### Overview

Returns `true` when exactly one of the Boolean-converted input and a secondary predicate expression evaluates to `true`. Null-like and unconvertible values convert to `false`; zero converts to `false`; nonzero numbers and recognized true text convert to `true`. Always evaluates the secondary expression.

###### Parameter
* expression: Specifies the secondary predicate expression evaluated after the input.

<!-- END AUTO-GENERATED -->
