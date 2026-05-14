---
title: Temporal predicates
subtitle: Predicates applicable to temporal values
tags: [predicates, temporal]
keywords: [after, after-or-same-instant, before, before-or-same-instant, business-day, contained-in, in-the-future, in-the-future-or-now, in-the-future-or-today, in-the-past, in-the-past-or-now, in-the-past-or-today, leap-year, on-the-day, on-the-hour, on-the-minute, same-instant, today, tomorrow, weekday, weekend, within-current-month, within-current-week, within-current-year, within-last-month, within-last-week, within-last-year, within-next-days, within-previous-days, within-upcoming-month, within-upcoming-week, within-upcoming-year, yesterday] # AUTO-GENERATED KEYWORDS
---
<!-- START AUTO-GENERATED -->
##### after

###### Alias: `dateTime-is-after`

###### Overview

Returns true if the temporal value passed as argument is chronologically after the temporal value passed as parameter. Returns `false` otherwise.

###### Parameter
* reference: A temporal value to compare to the argument.

##### after-or-same-instant

###### Alias: `dateTime-is-after-or-same-instant`

###### Overview

Returns true if the temporal value passed as argument is chronologically after the temporal value passed as parameter or if the two values represent the same instant . Returns `false` otherwise.

###### Parameter
* reference: A temporal value to compare to the argument.

##### before

###### Alias: `dateTime-is-before`

###### Overview

Returns true if the temporal value passed as argument is chronologically before the temporal value passed as parameter. Returns `false` otherwise.

###### Parameter
* reference: A temporal value to compare to the argument

##### before-or-same-instant

###### Alias: `dateTime-is-before-or-same-instant`

###### Overview

Returns true if the temporal value passed as argument is chronologically before the temporal value passed as parameter or if the two values represent the same instant . Returns `false` otherwise.

###### Parameter
* reference: A temporal value to compare to the argument.

##### business-day

###### Alias: `dateTime-is-business-day`

###### Overview

Returns `true` if the date passed as the argument doesn't correspond to a Saturday or a Sunday. Returns `false` otherwise.

##### contained-in

###### Alias: `dateTime-is-contained-in`

###### Overview

Returns true if the numeric value passed as argument is between the lower bound and the upper bound defined in the interval. Returns `false` otherwise.

###### Parameter
* interval: A temporal interval to compare to the argument.

##### in-the-future

###### Alias: `dateTime-is-in-the-future`

###### Overview

Returns true if the date passed as argument is after today. Returns false otherwise.

##### in-the-future-or-now

###### Alias: `dateTime-is-in-the-future-or-now`

###### Overview

Returns true if the dateTime passed as argument is after now. If a Date is passed as argument, it returns true if the date is today or after. Returns false otherwise.

##### in-the-future-or-today

###### Alias: `dateTime-is-in-the-future-or-today`

###### Overview

Returns true if the date passed as argument is today or a date after. If a DateTime is passed as argument, it must be today or after. Returns false otherwise.

##### in-the-past

###### Alias: `dateTime-is-in-the-past`

###### Overview

Returns true if the date passed as argument is before today. Returns false otherwise.

##### in-the-past-or-now

###### Alias: `dateTime-is-in-the-past-or-now`

###### Overview

Returns true if the dateTime passed as argument is before now. If a Date is passed as argument, it returns true if the date is today or before. Returns false otherwise.

##### in-the-past-or-today

###### Alias: `dateTime-is-in-the-past-or-today`

###### Overview

Returns true if the date passed as argument is today or a date before. If a DateTime is passed as argument, it returns true if the date of this datetime is today or any other date before today. Returns false otherwise.

##### leap-year

###### Alias: `dateTime-is-leap-year`

###### Overview

Returns true if the year of the dateTime value passed as the argument is a leap year. If the argument is not a dateTime but a numeric, returns true if the integer part of this value corresponds to a year that is a leap year. Returns false otherwise.

##### on-the-day

###### Alias: `dateTime-is-on-the-day`

###### Overview

Returns `true` if the argument is of type `DateOnly` or of type `DateTime` but the Time part is set at exactly midnight. Returns `false` otherwise.

##### on-the-hour

###### Alias: `dateTime-is-on-the-hour`

###### Overview

Returns `true` if the argument is of type `DateTime` and the minutes, seconds and milliseconds are all set at `0`. Returns `false` otherwise.

##### on-the-minute

###### Alias: `dateTime-is-on-the-minute`

###### Overview

Returns `true` if the argument is of type `DateTime` and the seconds and milliseconds are all set at `0`. Returns `false` otherwise.

##### same-instant

###### Alias: `dateTime-is-same-instant`

###### Overview

Returns true if the temporal value passed as argument is equal to the temporal value passed as parameter.

###### Parameter
* reference: A temporal value to compare to the argument.

##### today

###### Alias: `dateTime-is-today`

###### Overview

Returns true if the date passed as argument is representing the current date. Returns false otherwise.

##### tomorrow

###### Alias: `dateTime-is-tomorrow`

###### Overview

Returns true if the date passed as argument is representing the next date compared to the current date. Returns false otherwise.

##### weekday

###### Alias: `dateTime-is-weekday`

###### Overview

Returns `true` if the date passed as the argument corresponds to the weekday passed as the parameter. Returns `false` otherwise.

###### Parameter
* weekday: The day of week to compare to the argument.

##### weekend

###### Alias: `dateTime-is-weekend`

###### Overview

Returns `true` if the date passed as the argument corresponds to a Saturday or a Sunday. Returns `false` otherwise.

##### within-current-month

###### Alias: `dateTime-is-within-current-month`

###### Overview

Returns true if the date passed as argument is part of the same month than the current date. Returns false otherwise.

##### within-current-week

###### Alias: `dateTime-is-within-current-week`

###### Overview

Returns true if the date passed as argument is part of the same week than the current date. A week is starting on Monday and ending on Sunday. Returns false otherwise.

##### within-current-year

###### Alias: `dateTime-is-within-current-year`

###### Overview

Returns true if the date passed as argument is part of the same year than the current date. Returns false otherwise.

##### within-last-month

###### Alias: `dateTime-is-within-last-month`

###### Overview

Returns true if the date passed as argument is part of the month preceding than the current month. Returns false otherwise.

##### within-last-week

###### Alias: `dateTime-is-within-last-week`

###### Overview

Returns true if the date passed as argument is part of the week preceding the current week. A week is starting on Monday and ending on Sunday. Returns false otherwise.

##### within-last-year

###### Alias: `dateTime-is-within-last-year`

###### Overview

Returns true if the date passed as argument is part of the year preceding the current year. Returns false otherwise.

##### within-next-days

###### Alias: `dateTime-is-within-next-days`

###### Overview

Returns true if the date passed as argument is between tomorrow and the count of days after the current date. Returns false otherwise.

###### Parameter
* count: Count of days to move forward. A value of 1 is equivalent to the predicate `tomorrow` and a value of 0 will return false.

##### within-previous-days

###### Alias: `dateTime-is-within-previous-days`

###### Overview

Returns true if the date passed as argument is between the count of days before the current date and yesterday (both included). Returns false otherwise.

###### Parameter
* count: Count of days to move backward. A value of 1 is equivalent to the predicate `yesterday` and a value of 0 will return false.

##### within-upcoming-month

###### Alias: `dateTime-is-within-upcoming-month`

###### Overview

Returns true if the date passed as argument is part of the month following than the current month. Returns false otherwise.

##### within-upcoming-week

###### Alias: `dateTime-is-within-upcoming-week`

###### Overview

Returns true if the date passed as argument is part of the week following the current week. A week is starting on Monday and ending on Sunday. Returns false otherwise.

##### within-upcoming-year

###### Alias: `dateTime-is-within-upcoming-year`

###### Overview

Returns true if the date passed as argument is part of the year following the current year. Returns false otherwise.

##### yesterday

###### Alias: `dateTime-is-yesterday`

###### Overview

Returns true if the date passed as argument is representing the previous date compared to the current date. Returns false otherwise.

<!-- END AUTO-GENERATED -->
