---
title: Temporal functions
subtitle: Functions applicable to temporal values
tags: [functions, temporal]
keywords: [age, backward, ceiling-hour, ceiling-minute, clamp, datetime-to-date, first-in-month, first-of-month, first-of-year, floor-hour, floor-minute, forward, invalid-to-date, last-in-month, last-of-month, last-of-year, length-of-month, length-of-year, local-to-utc, next-business-days, next-day, next-month, next-weekday, next-weekday-or-same, next-year, null-to-date, previous-business-days, previous-day, previous-month, previous-weekday, previous-weekday-or-same, previous-year, set-time, utc-to-local] # AUTO-GENERATED KEYWORDS
---
<!-- START AUTO-GENERATED -->
##### age
###### Overview

Returns how many years separate the argument dateTime and now.

##### backward
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

##### first-in-month
###### Overview

Returns a new date value corresponding to the first occurrence of the weekday passed as a parameter of the month of the date passed as the argument.

###### Parameter
* weekday: The day of week to compare to the argument.

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

##### last-in-month
###### Overview

Returns a new dateTime value corresponding to the last occurrence of the weekday passed as a parameter of the month of the date passed as the argument.

###### Parameter
* weekday: The day of week to compare to the argument.

##### last-of-month
###### Overview

Returns the last day of the month of the same month/year than the argument dateTime.

##### last-of-year
###### Overview

Returns the 31st of December of the same year than the argument dateTime.

##### length-of-month
###### Overview

returns the count of days within the month of the dateTime value passed as the argument. 
            If the argument is not a dateTime but a text at format "YYYY-MM", it returns count of days of the month represented by this value. 
            It returns a value between 28 and 31 (depending of leap year and month).

##### length-of-year
###### Overview

Returns the count of days within the year of the dateTime value passed as the argument.
            If the argument is not a dateTime but an integer, returns count of days of the corresponding year.
            It returns 365 or 366 (for leap years).

##### local-to-utc
###### Overview

Returns the dateTime passed as argument and set in the time zone passed as parameter converted to UTC.

##### next-business-days
###### Overview

Returns a new date value corresponding to the date passed as the argument, counting forward the business days (being weekdays different of Saturday and Sunday) specified as the parameter. It always returns a business day, as such if the date passed as the argument is a weekend, it considers that this date was the Friday before the argument value.

###### Parameter
* count: The count of business days to move forward.

##### next-day
###### Overview

Returns the day immediately following the dateTime passed as argument value.

##### next-month
###### Overview

Returns the dateTime that adds a month to the dateTime passed as argument value.

##### next-weekday
###### Overview

Returns a new date value corresponding to the occurrence of the weekday, passed as a parameter, following the date passed as the argument.

###### Parameter
* weekday: The day of week to compare to the argument.

##### next-weekday-or-same
###### Overview

Returns a new date value corresponding to the occurrence of the weekday passed as a parameter following the date passed as the argument except if this date corresponds to the expected weekday then it returns this date.

###### Parameter
* weekday: The day of week to compare to the argument.

##### next-year
###### Overview

Returns the dateTime that adds a year to the dateTime passed as argument value.

##### null-to-date
###### Overview

Returns the dateTime argument except if the value is `null` then it returns the parameter value.

###### Parameter
* default: The dateTime to be returned if the argument is `null`.

##### previous-business-days
###### Overview

Returns a new date value corresponding to the date passed as the argument, counting backward the business days (being weekdays different of Saturday and Sunday) specified as the parameter. It always returns a business day, as such if the date passed as the argument is a weekend, it considers that this date was the Friday before the argument value.

###### Parameter
* count: The count of business days to move forward.

##### previous-day
###### Overview

Returns the dateTime that substract a day to the dateTime passed as argument value.

##### previous-month
###### Overview

Returns the dateTime that substract a month to the dateTime passed as argument value.

##### previous-weekday
###### Overview

Returns a new date value corresponding to the occurrence of the weekday passed as a parameter preceding the date passed as the argument.

###### Parameter
* weekday: The day of week to compare to the argument.

##### previous-weekday-or-same
###### Overview

Returns a new date value corresponding to the occurrence of the weekday passed as a parameter preceding the date passed as the argument except if this date corresponds to the expected weekday then it returns this date.

###### Parameter
* weekday: The day of week to compare to the argument.

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
