---
title: Temporal predicates
subtitle: Predicates applicable to temporal values
tags: [predicates, temporal]
keywords: [after, after-or-same-instant, before, before-or-same-instant, contained-in, leap-year, on-the-day, on-the-hour, on-the-minute, same-instant, today, tomorrow, yesterday] # AUTO-GENERATED KEYWORDS
---
<!-- START AUTO-GENERATED -->
##### after
###### Overview

Returns true if the temporal value passed as argument is chronologically after the temporal value passed as parameter. Returns `false` otherwise.

###### Parameter
* reference: A temporal value to compare to the argument

##### after-or-same-instant
###### Overview

Returns true if the temporal value passed as argument is chronologically after the temporal value passed as parameter or if the two values represent the same instant . Returns `false` otherwise.

###### Parameter
* reference: A temporal value to compare to the argument

##### before
###### Overview

Returns true if the temporal value passed as argument is chronologically before the temporal value passed as parameter. Returns `false` otherwise.

###### Parameter
* reference: A temporal value to compare to the argument

##### before-or-same-instant
###### Overview

Returns true if the temporal value passed as argument is chronologically before the temporal value passed as parameter or if the two values represent the same instant . Returns `false` otherwise.

###### Parameter
* reference: A temporal value to compare to the argument

##### contained-in
###### Overview

Returns true if the numeric value passed as argument is between the lower bound and the upper bound defined in the interval. Returns `false` otherwise.

###### Parameter
* interval: A temporal interval to compare to the argument

##### leap-year
###### Overview

Returns true if the year of the dateTime value passed as the argument is a leap year. If the argument is not a dateTime but a numeric, returns true if the integer part of this value corresponds to a year that is a leap year. Returns false otherwise.

##### on-the-day
###### Overview

Returns `true` if the argument is of type `DateOnly` or of type `DateTime` but the Time part is set at exactly midnight. Returns `false` otherwise.

##### on-the-hour
###### Overview

Returns `true` if the argument is of type `DateTime` and the minutes, seconds and milliseconds are all set at `0`. Returns `false` otherwise.

##### on-the-minute
###### Overview

Returns `true` if the argument is of type `DateTime` and the seconds and milliseconds are all set at `0`. Returns `false` otherwise.

##### same-instant
###### Overview

Returns true if the temporal value passed as argument is equal to the temporal value passed as parameter.

###### Parameter
* reference: A temporal value to compare to the argument

##### today
###### Overview

Returns true if the date passed as argument is representing the current date. Returns false otherwise.

##### tomorrow
###### Overview

Returns true if the date passed as argument is representing the next date compared to the current date. Returns false otherwise.

##### yesterday
###### Overview

Returns true if the date passed as argument is representing the previous date compared to the current date. Returns false otherwise.

<!-- END AUTO-GENERATED -->
