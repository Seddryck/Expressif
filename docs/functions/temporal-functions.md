---
layout: docs
title: "Temporal functions"
parent: "Functions library"

nav_order: 60
has_children: true
has_toc: false
permalink: /functions/temporal-functions/
tags:
  - functions
  - temporal

generated: true
---

Reference documentation for Expressif functions in the `temporal` scope.

| Name | Overview |
|:-----|:---------|
| [`age`]({{ '/functions/temporal/age/' | relative_url }}) | Returns the completed years between the argument dateTime and the current date. Returns `null` for null or future dates. In a non-leap year, a February 29 birthday is reached on February 28. |
| [`backward`]({{ '/functions/temporal/backward/' | relative_url }}) | Returns a dateTime that subtract the timestamp passed as parameter to the argument. If times is specified this operation is reproduced. |
| [`catholic-calendar`]({{ '/functions/temporal/calendar/catholic-calendar/' | relative_url }}) | Returns the date of the Catholic calendar event passed as parameter for the year specified by the argument. Returns `null` if the event is unknown. |
| [`ceiling-hour`]({{ '/functions/temporal/ceiling-hour/' | relative_url }}) | Returns the dateTime passed as argument value with the minutes, seconds and milliseconds set to zero and an hour added. |
| [`ceiling-minute`]({{ '/functions/temporal/ceiling-minute/' | relative_url }}) | Returns the dateTime passed as argument value with the seconds and milliseconds set to zero and a minute added. |
| [`change-of-hour`]({{ '/functions/temporal/change-of-hour/' | relative_url }}) | returns a temporal value corresponding to the same instant of the argument value but with a new value for the second part. |
| [`change-of-minute`]({{ '/functions/temporal/change-of-minute/' | relative_url }}) | returns a temporal value corresponding to the same instant of the argument value but with a new value for the second part. |
| [`change-of-month`]({{ '/functions/temporal/change-of-month/' | relative_url }}) | returns a temporal value corresponding to the same day and year of the argument value but of the month passed as the parameter. If the original day is 29, 30, or 31 and the new month passed as a parameter has fewer days then it returns the last day of the corresponding month. |
| [`change-of-second`]({{ '/functions/temporal/change-of-second/' | relative_url }}) | returns a temporal value corresponding to the same instant of the argument value but with a new value for the second part. |
| [`change-of-year`]({{ '/functions/temporal/change-of-year/' | relative_url }}) | returns a temporal value corresponding to the same day and month of the argument value but of the year passed as the parameter. If the original date was the 29th of February and the year passed as a parameter is not a leap year then it returns the 28th of February. |
| [`clamp`]({{ '/functions/temporal/clamp/' | relative_url }}) | Returns the value of an argument dateTime, unless it is before min (in which case it returns min), or after max (in which case it returns max). |
| [`datetime-to-date`]({{ '/functions/temporal/conversion/datetime-to-date/' | relative_url }}) | Returns the date at midnight of the argument dateTime. |
| [`day-of-month`]({{ '/functions/temporal/day-of-month/' | relative_url }}) | returns a numeric value representing the day of the month of the date passed as the argument |
| [`day-of-week`]({{ '/functions/temporal/day-of-week/' | relative_url }}) | returns a numeric value representing the day of the week (1 being Monday and 7 being Sunday) of the date passed as the argument |
| [`day-of-year`]({{ '/functions/temporal/day-of-year/' | relative_url }}) | returns a numeric value representing the day position within the year of the date passed as the argument |
| [`duration-between`]({{ '/functions/temporal/duration-between/' | relative_url }}) | Returns the signed duration between the current temporal value and a previous temporal value. Returns `null` when either value cannot be evaluated or the temporal values are incompatible. |
| [`first-in-month`]({{ '/functions/temporal/calendar/first-in-month/' | relative_url }}) | Returns a new date value corresponding to the first occurrence of the weekday passed as a parameter of the month of the date passed as the argument. |
| [`first-of-month`]({{ '/functions/temporal/calendar/first-of-month/' | relative_url }}) | Returns the first day of the month of the same month/year than the argument dateTime. |
| [`first-of-year`]({{ '/functions/temporal/calendar/first-of-year/' | relative_url }}) | Returns the first of January of the same year than the argument dateTime. |
| [`floor-hour`]({{ '/functions/temporal/floor-hour/' | relative_url }}) | Returns the dateTime passed as argument value with the minutes, seconds and milliseconds set to zero. |
| [`floor-minute`]({{ '/functions/temporal/floor-minute/' | relative_url }}) | Returns the dateTime passed as argument value with the seconds and milliseconds set to zero. |
| [`forward`]({{ '/functions/temporal/forward/' | relative_url }}) | Returns a dateTime that adds the timestamp passed as parameter to the argument. If times is specified this operation is reproduced. |
| [`hour`]({{ '/functions/temporal/hour/' | relative_url }}) | returns a textual value at format hh (24 hours format) representing the hours of the dateTime passed as the argument |
| [`hour-minute`]({{ '/functions/temporal/hour-minute/' | relative_url }}) | returns a textual value at format hh:mm (24 hours format) representing the hours and minutes of the dateTime passed as the argument |
| [`hour-minute-second`]({{ '/functions/temporal/hour-minute-second/' | relative_url }}) | returns a textual value at format hh:mm:ss (24 hours format) representing the hours, minutes, and seconds of the dateTime passed as the argument |
| [`hour-of-day`]({{ '/functions/temporal/hour-of-day/' | relative_url }}) | returns a numeric value representing the hours of the date passed as the argument |
| [`invalid-to-date`]({{ '/functions/temporal/conversion/invalid-to-date/' | relative_url }}) | Returns the dateTime argument except if the value is not a valid dateTime then it returns the parameter value. |
| [`iso-day-of-year`]({{ '/functions/temporal/iso-day-of-year/' | relative_url }}) | returns a numeric value representing the day position within the year of the date passed as the argument |
| [`iso-week-of-year`]({{ '/functions/temporal/iso-week-of-year/' | relative_url }}) | returns a textual value at format YYYY-Www representing the year and week number (according to ISO 8601) of the date passed as the argument |
| [`iso-year-day`]({{ '/functions/temporal/iso-year-day/' | relative_url }}) | returns a textual value at format YYYY-ddd representing the year, and the day number of the date passed as the argument (both according to ISO 8601) |
| [`iso-year-week`]({{ '/functions/temporal/iso-year-week/' | relative_url }}) | returns a textual value at format YYYY-Www representing the year and week number (according to ISO 8601) of the date passed as the argument |
| [`iso-year-week-day`]({{ '/functions/temporal/iso-year-week-day/' | relative_url }}) | returns a textual value at format YYYY-Www-D representing the year and week number (according to ISO 8601), and the day number (1 being Monday) of the date passed as the argument |
| [`last-in-month`]({{ '/functions/temporal/calendar/last-in-month/' | relative_url }}) | Returns a new dateTime value corresponding to the last occurrence of the weekday passed as a parameter of the month of the date passed as the argument. |
| [`last-of-month`]({{ '/functions/temporal/calendar/last-of-month/' | relative_url }}) | Returns the last day of the month of the same month/year than the argument dateTime. |
| [`last-of-year`]({{ '/functions/temporal/calendar/last-of-year/' | relative_url }}) | Returns the 31st of December of the same year than the argument dateTime. |
| [`length-of-month`]({{ '/functions/temporal/calendar/length-of-month/' | relative_url }}) | returns the count of days within the month of the dateTime value passed as the argument. If the argument is not a dateTime but a text at format "YYYY-MM", it returns count of days of the month represented by this value. It returns a value between 28 and 31 (depending of leap year and month). |
| [`length-of-year`]({{ '/functions/temporal/calendar/length-of-year/' | relative_url }}) | Returns the count of days within the year of the dateTime value passed as the argument. If the argument is not a dateTime but an integer, returns count of days of the corresponding year. It returns 365 or 366 (for leap years). |
| [`local-to-utc`]({{ '/functions/temporal/local-to-utc/' | relative_url }}) | Returns the dateTime passed as argument and set in the time zone passed as parameter converted to UTC. |
| [`minute-of-day`]({{ '/functions/temporal/minute-of-day/' | relative_url }}) | returns a numeric value representing the minutes of the date passed as the argument |
| [`minute-of-hour`]({{ '/functions/temporal/minute-of-hour/' | relative_url }}) | returns a numeric value representing the minutes of the hour passed as the argument |
| [`month`]({{ '/functions/temporal/month/' | relative_url }}) | returns a textual value at format MM representing the month of the date passed as the argument |
| [`month-day`]({{ '/functions/temporal/month-day/' | relative_url }}) | returns a textual value at format MM-DD representing the month and day of the date passed as the argument |
| [`month-of-year`]({{ '/functions/temporal/month-of-year/' | relative_url }}) | returns a numeric value representing the month of the date passed as the argument |
| [`next-business-days`]({{ '/functions/temporal/next-business-days/' | relative_url }}) | Returns a new date value corresponding to the date passed as the argument, counting forward the business days (being weekdays different of Saturday and Sunday) specified as the parameter. It always returns a business day, as such if the date passed as the argument is a weekend, it considers that this date was the Friday before the argument value. |
| [`next-day`]({{ '/functions/temporal/next-day/' | relative_url }}) | Returns the day immediately following the dateTime passed as argument value. |
| [`next-month`]({{ '/functions/temporal/next-month/' | relative_url }}) | Returns the dateTime that adds a month to the dateTime passed as argument value. |
| [`next-weekday`]({{ '/functions/temporal/next-weekday/' | relative_url }}) | Returns a new date value corresponding to the occurrence of the weekday, passed as a parameter, following the date passed as the argument. |
| [`next-weekday-or-same`]({{ '/functions/temporal/next-weekday-or-same/' | relative_url }}) | Returns a new date value corresponding to the occurrence of the weekday passed as a parameter following the date passed as the argument except if this date corresponds to the expected weekday then it returns this date. |
| [`next-year`]({{ '/functions/temporal/next-year/' | relative_url }}) | Returns the dateTime that adds a year to the dateTime passed as argument value. |
| [`null-to-date`]({{ '/functions/temporal/conversion/null-to-date/' | relative_url }}) | Returns the dateTime argument except if the value is `null` then it returns the parameter value. |
| [`previous-business-days`]({{ '/functions/temporal/previous-business-days/' | relative_url }}) | Returns a new date value corresponding to the date passed as the argument, counting backward the business days (being weekdays different of Saturday and Sunday) specified as the parameter. It always returns a business day, as such if the date passed as the argument is a weekend, it considers that this date was the Friday before the argument value. |
| [`previous-day`]({{ '/functions/temporal/previous-day/' | relative_url }}) | Returns the dateTime that substract a day to the dateTime passed as argument value. |
| [`previous-month`]({{ '/functions/temporal/previous-month/' | relative_url }}) | Returns the dateTime that substract a month to the dateTime passed as argument value. |
| [`previous-weekday`]({{ '/functions/temporal/previous-weekday/' | relative_url }}) | Returns a new date value corresponding to the occurrence of the weekday passed as a parameter preceding the date passed as the argument. |
| [`previous-weekday-or-same`]({{ '/functions/temporal/previous-weekday-or-same/' | relative_url }}) | Returns a new date value corresponding to the occurrence of the weekday passed as a parameter preceding the date passed as the argument except if this date corresponds to the expected weekday then it returns this date. |
| [`previous-year`]({{ '/functions/temporal/previous-year/' | relative_url }}) | Returns the dateTime that substract a year to the dateTime passed as argument value. |
| [`second-of-day`]({{ '/functions/temporal/second-of-day/' | relative_url }}) | returns a numeric value representing the seconds of the day of the date passed as the argument |
| [`second-of-hour`]({{ '/functions/temporal/second-of-hour/' | relative_url }}) | returns a numeric value representing the seconds of the hour of the date passed as the argument |
| [`second-of-minute`]({{ '/functions/temporal/second-of-minute/' | relative_url }}) | returns a numeric value representing the seconds of the minute of the date passed as the argument |
| [`set-time`]({{ '/functions/temporal/set-time/' | relative_url }}) | Returns a dateTime with the time part set to the value passed as parameter and the date part corresponding to the argument value. |
| [`set-to-local`]({{ '/functions/temporal/set-to-local/' | relative_url }}) | Returns the dateTime passed as argument without changing the current hours/minutes and sets the kind to local |
| [`set-to-utc`]({{ '/functions/temporal/set-to-utc/' | relative_url }}) | Returns the dateTime passed as argument without changing the current hours/minutes and sets the kind to UTC |
| [`utc-to-local`]({{ '/functions/temporal/utc-to-local/' | relative_url }}) | Returns the dateTime passed as argument and set in UTC converted to the time zone passed as parameter. |
| [`year`]({{ '/functions/temporal/year/' | relative_url }}) | returns a textual value at format YYYY representing the year of the date passed as the argument |
| [`year-of-era`]({{ '/functions/temporal/year-of-era/' | relative_url }}) | returns a numeric value representing the year of the date passed as the argument |
