---
title: Temporal predicates
subtitle: Predicates applicable to temporal values
tags: [predicates, temporal]
keywords: [is-after, is-after-or-same-instant, is-before, is-before-or-same-instant, is-business-day, is-contained-in, is-in-the-future, is-in-the-future-or-now, is-in-the-future-or-today, is-in-the-past, is-in-the-past-or-now, is-in-the-past-or-today, is-leap-year, is-on-the-day, is-on-the-hour, is-on-the-minute, is-same-instant, is-today, is-tomorrow, is-weekday, is-weekend, is-within-current-month, is-within-current-week, is-within-current-year, is-within-last-month, is-within-last-week, is-within-last-year, is-within-next-days, is-within-previous-days, is-within-upcoming-month, is-within-upcoming-week, is-within-upcoming-year, is-yesterday] # AUTO-GENERATED KEYWORDS
---
<!-- START AUTO-GENERATED -->
##### is-after

###### Aliases: `after`, `dateTime-is-after`

###### Overview

Returns true if the temporal value passed as argument is chronologically after the temporal value passed as parameter. Returns `false` otherwise.

###### Parameter
* reference: A temporal value to compare to the argument.

##### is-after-or-same-instant

###### Aliases: `after-or-same-instant`, `dateTime-is-after-or-same-instant`

###### Overview

Returns true if the temporal value passed as argument is chronologically after the temporal value passed as parameter or if the two values represent the same instant . Returns `false` otherwise.

###### Parameter
* reference: A temporal value to compare to the argument.

##### is-before

###### Aliases: `before`, `dateTime-is-before`

###### Overview

Returns true if the temporal value passed as argument is chronologically before the temporal value passed as parameter. Returns `false` otherwise.

###### Parameter
* reference: A temporal value to compare to the argument

##### is-before-or-same-instant

###### Aliases: `before-or-same-instant`, `dateTime-is-before-or-same-instant`

###### Overview

Returns true if the temporal value passed as argument is chronologically before the temporal value passed as parameter or if the two values represent the same instant . Returns `false` otherwise.

###### Parameter
* reference: A temporal value to compare to the argument.

##### is-business-day

###### Aliases: `business-day`, `dateTime-is-business-day`

###### Overview

Returns `true` if the date passed as the argument doesn't correspond to a Saturday or a Sunday. Returns `false` otherwise.

##### is-contained-in

###### Aliases: `contained-in`, `dateTime-is-contained-in`

###### Overview

Returns true if the temporal value passed as argument is between the lower bound and the upper bound defined in the interval. Returns `false` otherwise.

###### Parameter
* interval: A temporal interval to compare to the argument.

##### is-in-the-future

###### Aliases: `in-the-future`, `dateTime-is-in-the-future`

###### Overview

Returns true if the date passed as argument is after today. Returns false otherwise.

##### is-in-the-future-or-now

###### Aliases: `in-the-future-or-now`, `dateTime-is-in-the-future-or-now`

###### Overview

Returns true if the dateTime passed as argument is after now. If a Date is passed as argument, it returns true if the date is today or after. Returns false otherwise.

##### is-in-the-future-or-today

###### Aliases: `in-the-future-or-today`, `dateTime-is-in-the-future-or-today`

###### Overview

Returns true if the date passed as argument is today or a date after. If a DateTime is passed as argument, it must be today or after. Returns false otherwise.

##### is-in-the-past

###### Aliases: `in-the-past`, `dateTime-is-in-the-past`

###### Overview

Returns true if the date passed as argument is before today. Returns false otherwise.

##### is-in-the-past-or-now

###### Aliases: `in-the-past-or-now`, `dateTime-is-in-the-past-or-now`

###### Overview

Returns true if the dateTime passed as argument is before now. If a Date is passed as argument, it returns true if the date is today or before. Returns false otherwise.

##### is-in-the-past-or-today

###### Aliases: `in-the-past-or-today`, `dateTime-is-in-the-past-or-today`

###### Overview

Returns true if the date passed as argument is today or a date before. If a DateTime is passed as argument, it returns true if the date of this datetime is today or any other date before today. Returns false otherwise.

##### is-leap-year

###### Aliases: `leap-year`, `dateTime-is-leap-year`

###### Overview

Returns true if the year of the dateTime value passed as the argument is a leap year. If the argument is not a dateTime but a numeric, returns true if the integer part of this value corresponds to a year that is a leap year. Returns false otherwise.

##### is-on-the-day

###### Aliases: `on-the-day`, `dateTime-is-on-the-day`

###### Overview

Returns `true` if the argument is of type `DateOnly` or of type `DateTime` but the Time part is set at exactly midnight. Returns `false` otherwise.

##### is-on-the-hour

###### Aliases: `on-the-hour`, `dateTime-is-on-the-hour`

###### Overview

Returns `true` if the argument is of type `DateTime` and the minutes, seconds and milliseconds are all set at `0`. Returns `false` otherwise.

##### is-on-the-minute

###### Aliases: `on-the-minute`, `dateTime-is-on-the-minute`

###### Overview

Returns `true` if the argument is of type `DateTime` and the seconds and milliseconds are all set at `0`. Returns `false` otherwise.

##### is-same-instant

###### Aliases: `same-instant`, `dateTime-is-same-instant`

###### Overview

Returns true if the temporal value passed as argument is equal to the temporal value passed as parameter.

###### Parameter
* reference: A temporal value to compare to the argument.

##### is-today

###### Aliases: `today`, `dateTime-is-today`

###### Overview

Returns true if the date passed as argument is representing the current date. Returns false otherwise.

##### is-tomorrow

###### Aliases: `tomorrow`, `dateTime-is-tomorrow`

###### Overview

Returns true if the date passed as argument is representing the next date compared to the current date. Returns false otherwise.

##### is-weekday

###### Aliases: `weekday`, `dateTime-is-weekday`

###### Overview

Returns `true` if the date passed as the argument corresponds to the weekday passed as the parameter. Returns `false` otherwise.

###### Parameter
* weekday: The day of week to compare to the argument.

##### is-weekend

###### Aliases: `weekend`, `dateTime-is-weekend`

###### Overview

Returns `true` if the date passed as the argument corresponds to a Saturday or a Sunday. Returns `false` otherwise.

##### is-within-current-month

###### Aliases: `within-current-month`, `dateTime-is-within-current-month`

###### Overview

Returns true if the date passed as argument is part of the same month than the current date. Returns false otherwise.

##### is-within-current-week

###### Aliases: `within-current-week`, `dateTime-is-within-current-week`

###### Overview

Returns true if the date passed as argument is part of the same week than the current date. A week is starting on Monday and ending on Sunday. Returns false otherwise.

##### is-within-current-year

###### Aliases: `within-current-year`, `dateTime-is-within-current-year`

###### Overview

Returns true if the date passed as argument is part of the same year than the current date. Returns false otherwise.

##### is-within-last-month

###### Aliases: `within-last-month`, `dateTime-is-within-last-month`

###### Overview

Returns true if the date passed as argument is part of the month preceding than the current month. Returns false otherwise.

##### is-within-last-week

###### Aliases: `within-last-week`, `dateTime-is-within-last-week`

###### Overview

Returns true if the date passed as argument is part of the week preceding the current week. A week is starting on Monday and ending on Sunday. Returns false otherwise.

##### is-within-last-year

###### Aliases: `within-last-year`, `dateTime-is-within-last-year`

###### Overview

Returns true if the date passed as argument is part of the year preceding the current year. Returns false otherwise.

##### is-within-next-days

###### Aliases: `within-next-days`, `dateTime-is-within-next-days`

###### Overview

Returns true if the date passed as argument is between tomorrow and the count of days after the current date. Returns false otherwise.

###### Parameter
* count: Count of days to move forward. A value of 1 is equivalent to the predicate `tomorrow` and a value of 0 will return false.

##### is-within-previous-days

###### Aliases: `within-previous-days`, `dateTime-is-within-previous-days`

###### Overview

Returns true if the date passed as argument is between the count of days before the current date and yesterday (both included). Returns false otherwise.

###### Parameter
* count: Count of days to move backward. A value of 1 is equivalent to the predicate `yesterday` and a value of 0 will return false.

##### is-within-upcoming-month

###### Aliases: `within-upcoming-month`, `dateTime-is-within-upcoming-month`

###### Overview

Returns true if the date passed as argument is part of the month following than the current month. Returns false otherwise.

##### is-within-upcoming-week

###### Aliases: `within-upcoming-week`, `dateTime-is-within-upcoming-week`

###### Overview

Returns true if the date passed as argument is part of the week following the current week. A week is starting on Monday and ending on Sunday. Returns false otherwise.

##### is-within-upcoming-year

###### Aliases: `within-upcoming-year`, `dateTime-is-within-upcoming-year`

###### Overview

Returns true if the date passed as argument is part of the year following the current year. Returns false otherwise.

##### is-yesterday

###### Aliases: `yesterday`, `dateTime-is-yesterday`

###### Overview

Returns true if the date passed as argument is representing the previous date compared to the current date. Returns false otherwise.

<!-- END AUTO-GENERATED -->
