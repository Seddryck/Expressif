---
layout: docs
title: "Calendar functions"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 10
has_children: true
has_toc: false
permalink: /functions/temporal/calendar/
tags:
  - functions
  - temporal
  - calendar
generated: true
---

Reference documentation for Expressif functions in the `temporal/calendar` scope.

| Name | Overview |
|:-----|:---------|
| [`catholic-calendar`]({{ '/functions/temporal/calendar/catholic-calendar/' | relative_url }}) | Returns the date of the Catholic calendar event passed as parameter for the year specified by the argument. Returns `null` if the event is unknown. |
| [`first-in-month`]({{ '/functions/temporal/calendar/first-in-month/' | relative_url }}) | Returns a new date value corresponding to the first occurrence of the weekday passed as a parameter of the month of the date passed as the argument. |
| [`first-of-month`]({{ '/functions/temporal/calendar/first-of-month/' | relative_url }}) | Returns the first day of the month of the same month/year than the argument dateTime. |
| [`first-of-year`]({{ '/functions/temporal/calendar/first-of-year/' | relative_url }}) | Returns the first of January of the same year than the argument dateTime. |
| [`last-in-month`]({{ '/functions/temporal/calendar/last-in-month/' | relative_url }}) | Returns a new dateTime value corresponding to the last occurrence of the weekday passed as a parameter of the month of the date passed as the argument. |
| [`last-of-month`]({{ '/functions/temporal/calendar/last-of-month/' | relative_url }}) | Returns the last day of the month of the same month/year than the argument dateTime. |
| [`last-of-year`]({{ '/functions/temporal/calendar/last-of-year/' | relative_url }}) | Returns the 31st of December of the same year than the argument dateTime. |
| [`length-of-month`]({{ '/functions/temporal/calendar/length-of-month/' | relative_url }}) | returns the count of days within the month of the dateTime value passed as the argument. If the argument is not a dateTime but a text at format "YYYY-MM", it returns count of days of the month represented by this value. It returns a value between 28 and 31 (depending of leap year and month). |
| [`length-of-year`]({{ '/functions/temporal/calendar/length-of-year/' | relative_url }}) | Returns the count of days within the year of the dateTime value passed as the argument. If the argument is not a dateTime but an integer, returns count of days of the corresponding year. It returns 365 or 366 (for leap years). |
| [`next-business-days`]({{ '/functions/temporal/calendar/next-business-days/' | relative_url }}) | Returns a new date value corresponding to the date passed as the argument, counting forward the business days (being weekdays different of Saturday and Sunday) specified as the parameter. It always returns a business day, as such if the date passed as the argument is a weekend, it considers that this date was the Friday before the argument value. |
| [`next-weekday`]({{ '/functions/temporal/calendar/next-weekday/' | relative_url }}) | Returns a new date value corresponding to the occurrence of the weekday, passed as a parameter, following the date passed as the argument. |
| [`next-weekday-or-same`]({{ '/functions/temporal/calendar/next-weekday-or-same/' | relative_url }}) | Returns a new date value corresponding to the occurrence of the weekday passed as a parameter following the date passed as the argument except if this date corresponds to the expected weekday then it returns this date. |
| [`previous-business-days`]({{ '/functions/temporal/calendar/previous-business-days/' | relative_url }}) | Returns a new date value corresponding to the date passed as the argument, counting backward the business days (being weekdays different of Saturday and Sunday) specified as the parameter. It always returns a business day, as such if the date passed as the argument is a weekend, it considers that this date was the Friday before the argument value. |
| [`previous-weekday`]({{ '/functions/temporal/calendar/previous-weekday/' | relative_url }}) | Returns a new date value corresponding to the occurrence of the weekday passed as a parameter preceding the date passed as the argument. |
| [`previous-weekday-or-same`]({{ '/functions/temporal/calendar/previous-weekday-or-same/' | relative_url }}) | Returns a new date value corresponding to the occurrence of the weekday passed as a parameter preceding the date passed as the argument except if this date corresponds to the expected weekday then it returns this date. |
