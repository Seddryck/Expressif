---
title: Temporal functions
subtitle: Functions applicable to temporal values
tags: [functions, temporal]
keywords: [age, backward, catholic-calendar, ceiling-hour, ceiling-minute, change-of-hour, change-of-minute, change-of-month, change-of-second, change-of-year, clamp, datetime-to-date, day-of-month, day-of-week, day-of-year, first-in-month, first-of-month, first-of-year, floor-hour, floor-minute, forward, hour, hour-minute, hour-minute-second, hour-of-day, invalid-to-date, iso-day-of-year, iso-week-of-year, iso-year-day, iso-year-week, iso-year-week-day, last-in-month, last-of-month, last-of-year, length-of-month, length-of-year, local-to-utc, minute-of-day, minute-of-hour, month, month-day, month-of-year, next-business-days, next-day, next-month, next-weekday, next-weekday-or-same, next-year, null-to-date, previous-business-days, previous-day, previous-month, previous-weekday, previous-weekday-or-same, previous-year, second-of-day, second-of-hour, second-of-minute, set-time, set-to-local, set-to-utc, utc-to-local, year, year-of-era] # AUTO-GENERATED KEYWORDS
---
<!-- START AUTO-GENERATED -->
##### age

###### Alias: `date-to-age`

###### Overview

Returns how many years separate the argument dateTime and now.

##### backward

###### Aliases: `dateTime-to-backward`, `dateTime-to-subtract`

###### Overview

Returns a dateTime that subtract the timestamp passed as parameter to the argument. If times is specified this operation is reproduced.

###### Parameters
* timeSpan: The value to be subtracted to the argument value.
* times (optional) : An integer between 0 and +Infinity, indicating the number of times to repeat the subtraction

##### catholic-calendar

###### Alias: `calendar-catholic`

###### Overview

Returns the date of the Catholic calendar event passed as parameter for the year specified by the argument.
            Returns `null` if the event is unknown.

##### ceiling-hour

###### Alias: `dateTime-to-ceiling-hour`

###### Overview

Returns the dateTime passed as argument value with the minutes, seconds and milliseconds set to zero and an hour added.

##### ceiling-minute

###### Alias: `dateTime-to-ceiling-minute`

###### Overview

Returns the dateTime passed as argument value with the seconds and milliseconds set to zero and a minute added.

##### change-of-hour

###### Alias: `dateTime-to-change-of-hour`

###### Overview

returns a temporal value corresponding to the same instant of the argument value but with a new value for the second part.

##### change-of-minute

###### Alias: `dateTime-to-change-of-minute`

###### Overview

returns a temporal value corresponding to the same instant of the argument value but with a new value for the second part.

##### change-of-month

###### Alias: `dateTime-to-change-of-month`

###### Overview

returns a temporal value corresponding to the same day and year of the argument value but of the month passed as the parameter.
            If the original day is 29, 30, or 31 and the new month passed as a parameter has fewer days then it returns the last day of the corresponding month.

##### change-of-second

###### Alias: `dateTime-to-change-of-second`

###### Overview

returns a temporal value corresponding to the same instant of the argument value but with a new value for the second part.

##### change-of-year

###### Alias: `dateTime-to-change-of-year`

###### Overview

returns a temporal value corresponding to the same day and month of the argument value but of the year passed as the parameter.
            If the original date was the 29th of February and the year passed as a parameter is not a leap year then it returns the 28th of February.

##### clamp

###### Aliases: `dateTime-to-clamp`, `dateTime-to-clip`

###### Overview

Returns the value of an argument dateTime, unless it is before min (in which case it returns min), or after max (in which case it returns max).

###### Parameters
* min: value returned in case the argument value is before than it
* max: value returned in case the argument value is after than it

##### datetime-to-date

###### Alias: `dateTime-to-date`

###### Overview

Returns the date at midnight of the argument dateTime.

##### day-of-month

###### Alias: `dateTime-to-day-of-month`

###### Overview

returns a numeric value representing the day of the month of the date passed as the argument

##### day-of-week

###### Alias: `dateTime-to-day-of-week`

###### Overview

returns a numeric value representing the day of the week (1 being Monday and 7 being Sunday) of the date passed as the argument

##### day-of-year

###### Alias: `dateTime-to-day-of-year`

###### Overview

returns a numeric value representing the day position within the year of the date passed as the argument

##### first-in-month

###### Alias: `dateTime-to-first-in-month`

###### Overview

Returns a new date value corresponding to the first occurrence of the weekday passed as a parameter of the month of the date passed as the argument.

###### Parameter
* weekday: The day of week to compare to the argument.

##### first-of-month

###### Alias: `dateTime-to-first-of-month`

###### Overview

Returns the first day of the month of the same month/year than the argument dateTime.

##### first-of-year

###### Alias: `dateTime-to-first-of-year`

###### Overview

Returns the first of January of the same year than the argument dateTime.

##### floor-hour

###### Alias: `dateTime-to-floor-hour`

###### Overview

Returns the dateTime passed as argument value with the minutes, seconds and milliseconds set to zero.

##### floor-minute

###### Alias: `dateTime-to-floor-minute`

###### Overview

Returns the dateTime passed as argument value with the seconds and milliseconds set to zero.

##### forward

###### Aliases: `dateTime-to-forward`, `dateTime-to-add`

###### Overview

Returns a dateTime that adds the timestamp passed as parameter to the argument. If times is specified this operation is reproduced.

###### Parameters
* timeSpan: The value to be added to the argument value
* times (optional) : An integer between 0 and +Infinity, indicating the number of times to repeat the addition

##### hour

###### Alias: `dateTime-to-hour`

###### Overview

returns a textual value at format hh (24 hours format) representing the hours of the dateTime passed as the argument

##### hour-minute

###### Alias: `dateTime-to-hour-minute`

###### Overview

returns a textual value at format hh:mm (24 hours format) representing the hours and minutes of the dateTime passed as the argument

##### hour-minute-second

###### Alias: `dateTime-to-hour-minute-second`

###### Overview

returns a textual value at format hh:mm:ss (24 hours format) representing the hours, minutes, and seconds of the dateTime passed as the argument

##### hour-of-day

###### Alias: `dateTime-to-hour-of-day`

###### Overview

returns a numeric value representing the hours of the date passed as the argument

##### invalid-to-date
###### Overview

Returns the dateTime argument except if the value is not a valid dateTime then it returns the parameter value.

###### Parameter
* default: The dateTime to be returned if the argument is not a valid dateTime.

##### iso-day-of-year

###### Alias: `dateTime-to-iso-day-of-year`

###### Overview

returns a numeric value representing the day position within the year of the date passed as the argument

##### iso-week-of-year

###### Alias: `dateTime-to-iso-week-of-year`

###### Overview

returns a textual value at format YYYY-Www representing the year and week number (according to ISO 8601) of the date passed as the argument

##### iso-year-day

###### Alias: `dateTime-to-iso-year-day`

###### Overview

returns a textual value at format YYYY-ddd representing the year,
             and the day number of the date passed as the argument (both according to ISO 8601)

##### iso-year-week

###### Alias: `dateTime-to-iso-year-week`

###### Overview

returns a textual value at format YYYY-Www representing the year and week number (according to ISO 8601) of the date passed as the argument

##### iso-year-week-day

###### Alias: `dateTime-to-iso-year-week-day`

###### Overview

returns a textual value at format YYYY-Www-D representing the year and week number (according to ISO 8601),
            and the day number (1 being Monday) of the date passed as the argument

##### last-in-month

###### Alias: `dateTime-to-last-in-month`

###### Overview

Returns a new dateTime value corresponding to the last occurrence of the weekday passed as a parameter of the month of the date passed as the argument.

###### Parameter
* weekday: The day of week to compare to the argument.

##### last-of-month

###### Alias: `dateTime-to-last-of-month`

###### Overview

Returns the last day of the month of the same month/year than the argument dateTime.

##### last-of-year

###### Alias: `dateTime-to-last-of-year`

###### Overview

Returns the 31st of December of the same year than the argument dateTime.

##### length-of-month

###### Alias: `dateTime-to-length-of-month`

###### Overview

returns the count of days within the month of the dateTime value passed as the argument. 
            If the argument is not a dateTime but a text at format "YYYY-MM", it returns count of days of the month represented by this value. 
            It returns a value between 28 and 31 (depending of leap year and month).

##### length-of-year

###### Alias: `dateTime-to-length-of-year`

###### Overview

Returns the count of days within the year of the dateTime value passed as the argument.
            If the argument is not a dateTime but an integer, returns count of days of the corresponding year.
            It returns 365 or 366 (for leap years).

##### local-to-utc
###### Overview

Returns the dateTime passed as argument and set in the time zone passed as parameter converted to UTC.

##### minute-of-day

###### Alias: `dateTime-to-minute-of-day`

###### Overview

returns a numeric value representing the minutes of the date passed as the argument

##### minute-of-hour

###### Alias: `dateTime-to-minute-of-hour`

###### Overview

returns a numeric value representing the minutes of the hour passed as the argument

##### month

###### Alias: `dateTime-to-month`

###### Overview

returns a textual value at format MM representing the month of the date passed as the argument

##### month-day

###### Alias: `dateTime-to-month-day`

###### Overview

returns a textual value at format MM-DD representing the month and day of the date passed as the argument

##### month-of-year

###### Alias: `dateTime-to-month-of-year`

###### Overview

returns a numeric value representing the month of the date passed as the argument

##### next-business-days

###### Aliases: `temporal-to-next-business-days`, `next-business-day`, `add-business-days`

###### Overview

Returns a new date value corresponding to the date passed as the argument, counting forward the business days (being weekdays different of Saturday and Sunday) specified as the parameter. It always returns a business day, as such if the date passed as the argument is a weekend, it considers that this date was the Friday before the argument value.

###### Parameter
* count: The count of business days to move forward.

##### next-day

###### Alias: `dateTime-to-next-day`

###### Overview

Returns the day immediately following the dateTime passed as argument value.

##### next-month

###### Alias: `dateTime-to-next-month`

###### Overview

Returns the dateTime that adds a month to the dateTime passed as argument value.

##### next-weekday

###### Alias: `dateTime-to-next-weekday`

###### Overview

Returns a new date value corresponding to the occurrence of the weekday, passed as a parameter, following the date passed as the argument.

###### Parameter
* weekday: The day of week to compare to the argument.

##### next-weekday-or-same

###### Alias: `dateTime-to-next-weekday-or-same`

###### Overview

Returns a new date value corresponding to the occurrence of the weekday passed as a parameter following the date passed as the argument except if this date corresponds to the expected weekday then it returns this date.

###### Parameter
* weekday: The day of week to compare to the argument.

##### next-year

###### Alias: `dateTime-to-next-year`

###### Overview

Returns the dateTime that adds a year to the dateTime passed as argument value.

##### null-to-date
###### Overview

Returns the dateTime argument except if the value is `null` then it returns the parameter value.

###### Parameter
* default: The dateTime to be returned if the argument is `null`.

##### previous-business-days

###### Aliases: `temporal-to-previous-business-days`, `previous-business-day`, `subtract-business-days`

###### Overview

Returns a new date value corresponding to the date passed as the argument, counting backward the business days (being weekdays different of Saturday and Sunday) specified as the parameter. It always returns a business day, as such if the date passed as the argument is a weekend, it considers that this date was the Friday before the argument value.

###### Parameter
* count: The count of business days to move forward.

##### previous-day

###### Alias: `dateTime-to-previous-day`

###### Overview

Returns the dateTime that substract a day to the dateTime passed as argument value.

##### previous-month

###### Alias: `dateTime-to-previous-month`

###### Overview

Returns the dateTime that substract a month to the dateTime passed as argument value.

##### previous-weekday

###### Alias: `dateTime-to-previous-weekday`

###### Overview

Returns a new date value corresponding to the occurrence of the weekday passed as a parameter preceding the date passed as the argument.

###### Parameter
* weekday: The day of week to compare to the argument.

##### previous-weekday-or-same

###### Alias: `dateTime-to-previous-weekday-or-same`

###### Overview

Returns a new date value corresponding to the occurrence of the weekday passed as a parameter preceding the date passed as the argument except if this date corresponds to the expected weekday then it returns this date.

###### Parameter
* weekday: The day of week to compare to the argument.

##### previous-year

###### Alias: `dateTime-to-previous-year`

###### Overview

Returns the dateTime that substract a year to the dateTime passed as argument value.

##### second-of-day

###### Alias: `dateTime-to-second-of-day`

###### Overview

returns a numeric value representing the seconds of the day of the date passed as the argument

##### second-of-hour

###### Alias: `dateTime-to-second-of-hour`

###### Overview

returns a numeric value representing the seconds of the hour of the date passed as the argument

##### second-of-minute

###### Alias: `dateTime-to-second-of-minute`

###### Overview

returns a numeric value representing the seconds of the minute of the date passed as the argument

##### set-time

###### Alias: `dateTime-to-set-time`

###### Overview

Returns a dateTime with the time part set to the value passed as parameter and the date part corresponding to the argument value.

###### Parameter
* instant: The time value to set as hours, minutes, seconds of the dateTime argument

##### set-to-local
###### Overview

Returns the dateTime passed as argument without changing the current hours/minutes and sets the kind to local

##### set-to-utc
###### Overview

Returns the dateTime passed as argument without changing the current hours/minutes and sets the kind to UTC

##### utc-to-local
###### Overview

Returns the dateTime passed as argument and set in UTC converted to the time zone passed as parameter.

##### year

###### Alias: `dateTime-to-year`

###### Overview

returns a textual value at format YYYY representing the year of the date passed as the argument

##### year-of-era

###### Alias: `dateTime-to-year-of-era`

###### Overview

returns a numeric value representing the year of the date passed as the argument

<!-- END AUTO-GENERATED -->
