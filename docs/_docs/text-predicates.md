---
title: Text predicates
subtitle: Predicates applicable to text values
tags: [predicates, text]
keywords: [any-of, contains, empty, empty-or-null, ends-with, equivalent-to, lower-case, matches-date, matches-datetime, matches-numeric, matches-regex, matches-time, sorted-after, sorted-after-or-equivalent-to, sorted-before, sorted-before-or-equivalent-to, starts-with, upper-case] # AUTO-GENERATED KEYWORDS
---
<!-- START AUTO-GENERATED -->
##### any-of
###### Overview

Returns `true` if the list of text values passed as parameter contains the text value passed as argument. Returns `false` otherwise.

###### Parameters
* references: An array of text values
* comparer (optional) : 

##### contains
###### Overview

Returns `true` if the value passed as argument contains, anywhere in the string, the text value passed as parameter. Returns `false` otherwise.

###### Parameters
* reference: A string to be compared to the argument value
* comparer (optional) : A definition of the parameters of the comparison (case-sensitivity, culture-sensitivity)

##### empty
###### Overview

Returns `true` if argument value has a length of `0`. Return `false` otherwise.

##### empty-or-null
###### Overview

Returns `true` if argument value has a length of `0` or is `null`. Return `false` otherwise.

##### ends-with
###### Overview

Returns `true` if the value passed as argument ends with the text value passed as parameter. Returns `false` otherwise.

###### Parameters
* reference: A string to be compared to the argument value
* comparer (optional) : A definition of the parameters of the comparison (case-sensitivity, culture-sensitivity)

##### equivalent-to
###### Overview

Compare the text value passed as argument and the text value passed as parameter and returns `true` if they are equal. By default the comparison is agnostic of the culture and case-insensitive.

###### Parameters
* reference: A string to be compared to the argument value
* comparer (optional) : A definition of the parameters of the comparison (case-sensitivity, culture-sensitivity)

##### lower-case
###### Overview

Returns `true` if all characters of the text value passed as argument are lower-case. The value `null`, `empty` and `whitespace` also returns `true`. Returns `false` otherwise.

##### matches-date
###### Overview

Returns `true` if the text value passed as argument is a valid representation of a date in the culture specified as parameter. If the value is of type `DateTime` and the time part is set to midnight then it returns `true`. If the value is of type `Date`. Returns `false` otherwise.

##### matches-datetime
###### Overview

Returns `true` if the text value passed as argument is a valid representation of a dateTime in the culture specified as parameter. The expected format is the concatenation of the ShortDatePattern, a space and the LongTimePattern. If the value is of type `DateTime`, it returns `true`. Returns `false` otherwise.

##### matches-numeric
###### Overview

Returns `true` if the text value passed as argument is a valid representation of a numeric in the culture specified as parameter. Returns `false` otherwise.

##### matches-regex
###### Overview

Returns `true` if the value passed as argument validate the regex passed as parameter. Returns `false` otherwise.

###### Parameters
* regex: A string to be compared to the argument value
* comparer (optional) : A definition of the parameters of the comparison (case-sensitivity, culture-sensitivity)

##### matches-time
###### Overview

Returns `true` if the text value passed as argument is a valid representation of a time in the culture specified as parameter. The expected format is the LongTimePattern. If the value is of type `TimeOnly`, it returns `true`. Returns `false` otherwise.

##### sorted-after
###### Overview

Compare the text value passed as argument and the text value passed as parameter and returns `true` if argument value is alphabetically sorted after the parameter value. By default the comparison is agnostic of the culture and case-insensitive.

###### Parameters
* reference: A string to be compared to the argument value
* comparer (optional) : A definition of the parameters of the comparison (case-sensitivity, culture-sensitivity)

##### sorted-after-or-equivalent-to
###### Overview

Compare the text value passed as argument and the text value passed as parameter and returns `true` if argument value is alphabetically sorted after the parameter value or if the two values are equal. By default the comparison is agnostic of the culture and case-insensitive.///

###### Parameters
* reference: A string to be compared to the argument value
* comparer (optional) : A definition of the parameters of the comparison (case-sensitivity, culture-sensitivity)

##### sorted-before
###### Overview

Compare the text value passed as argument and the text value passed as parameter and returns `true` if argument value is alphabetically sorted before the parameter value. By default the comparison is agnostic of the culture and case-insensitive.

###### Parameters
* reference: A string to be compared to the argument value
* comparer (optional) : A definition of the parameters of the comparison (case-sensitivity, culture-sensitivity)

##### sorted-before-or-equivalent-to
###### Overview

Compare the text value passed as argument and the text value passed as parameter and returns `true` if argument value is alphabetically sorted before the parameter value or if the two values are equal. By default the comparison is agnostic of the culture and case-insensitive.

###### Parameters
* reference: A string to be compared to the argument value
* comparer (optional) : A definition of the parameters of the comparison (case-sensitivity, culture-sensitivity)

##### starts-with
###### Overview

Returns `true` if the value passed as argument starts with the text value passed as parameter. Returns `false` otherwise.

###### Parameters
* reference: A string to be compared to the argument value
* comparer (optional) : A definition of the parameters of the comparison (case-sensitivity, culture-sensitivity)

##### upper-case
###### Overview

Returns `true` if all characters of the text value passed as argument are upper-case. The value `null`, `empty` and `whitespace` also returns `true`. Returns `false` otherwise.

<!-- END AUTO-GENERATED -->
