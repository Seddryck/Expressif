---
title: Temporal functions
subtitle: Functions applicable to temporal values
tags: [functions, temporal]
---
<!-- START AUTO-GENERATED -->
### age

Returns how many years separate the argument dateTime and now.

### back

Returns a dateTime that subtract the timestamp passed as parameter to the argument. If times is specified this operation is reproduced.

### ceiling-hour

Returns the dateTime passed as argument value with the minutes, seconds and milliseconds set to zero and an hour added.

### ceiling-minute

Returns the dateTime passed as argument value with the seconds and milliseconds set to zero and a minute added.

### clamp

Returns the value of an argument dateTime, unless it is before min (in which case it returns min), or after max (in which case it returns max).

### datetime-to-date

Returns the date at midnight of the argument dateTime.

### first-of-month

Returns the first day of the month of the same month/year than the argument dateTime.

### first-of-year

Returns the first of January of the same year than the argument dateTime.

### floor-hour

Returns the dateTime passed as argument value with the minutes, seconds and milliseconds set to zero.

### floor-minute

Returns the dateTime passed as argument value with the seconds and milliseconds set to zero.

### forward

Returns a dateTime that adds the timestamp passed as parameter to the argument. If times is specified this operation is reproduced.

### invalid-to-date

Returns the dateTime argument except if the value is not a valid dateTime then it returns the parameter value.

### last-of-month

Returns the last day of the month of the same month/year than the argument dateTime.

### last-of-year

Returns the 31st of December of the same year than the argument dateTime.

### local-to-utc

Returns the dateTime passed as argument and set in the time zone passed as parameter converted to UTC.

### next-day

Returns the day immediately following the dateTime passed as argument value.

### next-month

Returns the dateTime that adds a month to the dateTime passed as argument value.

### next-year

Returns the dateTime that adds a year to the dateTime passed as argument value.

### null-to-date

Returns the dateTime argument except if the value is `null` then it returns the parameter value.

### previous-day

Returns the dateTime that substract a day to the dateTime passed as argument value.

### previous-month

Returns the dateTime that substract a month to the dateTime passed as argument value.

### previous-year

Returns the dateTime that substract a year to the dateTime passed as argument value.

### set-time

Returns a dateTime with the time part set to the value passed as parameter and the date part corresponding to the argument value.

### utc-to-local

Returns the dateTime passed as argument and set in UTC converted to the time zone passed as parameter.

<!-- END AUTO-GENERATED -->
