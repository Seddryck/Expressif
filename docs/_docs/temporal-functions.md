---
title: Temporal functions
subtitle: Functions applicable to temporal values
tags: [functions, temporal]
---
<!-- START AUTO-GENERATED -->
##### age
###### Overview

Returns how many years separate the argument dateTime and now.

##### back
###### Overview

Returns a dateTime that subtract the timestamp passed as parameter to the argument. If times is specified this operation is reproduced.

###### Parameters
* timeSpan: The value to be subtracted to the argument value.
* times (optional) : An integer between 0 and +Infinity, indicating the number of times to repeat the subtraction

##### ceiling-hour
###### Overview

Returns the dateTime passed as argument value with the minutes, seconds and milliseconds set to zero and an hour added.

##### ceiling-minute
###### Overview

Returns the dateTime passed as argument value with the seconds and milliseconds set to zero and a minute added.

##### clamp
###### Overview

Returns the value of an argument dateTime, unless it is before min (in which case it returns min), or after max (in which case it returns max).

###### Parameters
* min: value returned in case the argument value is before than it
* max: value returned in case the argument value is after than it

##### datetime-to-date
###### Overview

Returns the date at midnight of the argument dateTime.

##### first-of-month
###### Overview

Returns the first day of the month of the same month/year than the argument dateTime.

##### first-of-year
###### Overview

Returns the first of January of the same year than the argument dateTime.

##### floor-hour
###### Overview

Returns the dateTime passed as argument value with the minutes, seconds and milliseconds set to zero.

##### floor-minute
###### Overview

Returns the dateTime passed as argument value with the seconds and milliseconds set to zero.

##### forward
###### Overview

Returns a dateTime that adds the timestamp passed as parameter to the argument. If times is specified this operation is reproduced.

###### Parameters
* timeSpan: The value to be added to the argument value
* times (optional) : An integer between 0 and +Infinity, indicating the number of times to repeat the addition

##### invalid-to-date
###### Overview

Returns the dateTime argument except if the value is not a valid dateTime then it returns the parameter value.

###### Parameter
* default: The dateTime to be returned if the argument is not a valid dateTime.

##### last-of-month
###### Overview

Returns the last day of the month of the same month/year than the argument dateTime.

##### last-of-year
###### Overview

Returns the 31st of December of the same year than the argument dateTime.

##### local-to-utc
###### Overview

Returns the dateTime passed as argument and set in the time zone passed as parameter converted to UTC.

##### next-day
###### Overview

Returns the day immediately following the dateTime passed as argument value.

##### next-month
###### Overview

Returns the dateTime that adds a month to the dateTime passed as argument value.

##### next-year
###### Overview

Returns the dateTime that adds a year to the dateTime passed as argument value.

##### null-to-date
###### Overview

Returns the dateTime argument except if the value is `null` then it returns the parameter value.

###### Parameter
* default: The dateTime to be returned if the argument is `null`.

##### previous-day
###### Overview

Returns the dateTime that substract a day to the dateTime passed as argument value.

##### previous-month
###### Overview

Returns the dateTime that substract a month to the dateTime passed as argument value.

##### previous-year
###### Overview

Returns the dateTime that substract a year to the dateTime passed as argument value.

##### set-time
###### Overview

Returns a dateTime with the time part set to the value passed as parameter and the date part corresponding to the argument value.

###### Parameter
* instant: The time value to set as hours, minutes, seconds of the dateTime argument

##### utc-to-local
###### Overview

Returns the dateTime passed as argument and set in UTC converted to the time zone passed as parameter.

<!-- END AUTO-GENERATED -->
