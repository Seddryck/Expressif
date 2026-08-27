---
layout: docs
title: "Temporal predicates"
parent: "Predicates library"

nav_order: 50
has_children: true
has_toc: false
permalink: /predicates/temporal-predicates/
tags:
  - predicates
  - temporal

generated: true
---

Reference documentation for Expressif predicates in the `temporal` scope.

| Name | Overview |
|:-----|:---------|
| [`is-after`]({{ '/predicates/temporal/is-after/' | relative_url }}) | Returns true if the temporal value passed as argument is chronologically after the temporal value passed as parameter. Returns `false` otherwise. |
| [`is-after-or-same-instant`]({{ '/predicates/temporal/is-after-or-same-instant/' | relative_url }}) | Returns true if the temporal value passed as argument is chronologically after the temporal value passed as parameter or if the two values represent the same instant . Returns `false` otherwise. |
| [`is-before`]({{ '/predicates/temporal/is-before/' | relative_url }}) | Returns true if the temporal value passed as argument is chronologically before the temporal value passed as parameter. Returns `false` otherwise. |
| [`is-before-or-same-instant`]({{ '/predicates/temporal/is-before-or-same-instant/' | relative_url }}) | Returns true if the temporal value passed as argument is chronologically before the temporal value passed as parameter or if the two values represent the same instant . Returns `false` otherwise. |
| [`is-business-day`]({{ '/predicates/temporal/is-business-day/' | relative_url }}) | Returns `true` if the date passed as the argument doesn't correspond to a Saturday or a Sunday. Returns `false` otherwise. |
| [`is-contained-in`]({{ '/predicates/temporal/is-contained-in/' | relative_url }}) | Returns true if the temporal value passed as argument is between the lower bound and the upper bound defined in the interval. Returns `false` otherwise. |
| [`is-in-the-future`]({{ '/predicates/temporal/is-in-the-future/' | relative_url }}) | Returns true if the date passed as argument is after today. Returns false otherwise. |
| [`is-in-the-future-or-now`]({{ '/predicates/temporal/is-in-the-future-or-now/' | relative_url }}) | Returns true if the dateTime passed as argument is after now. If a Date is passed as argument, it returns true if the date is today or after. Returns false otherwise. |
| [`is-in-the-future-or-today`]({{ '/predicates/temporal/is-in-the-future-or-today/' | relative_url }}) | Returns true if the date passed as argument is today or a date after. If a DateTime is passed as argument, it must be today or after. Returns false otherwise. |
| [`is-in-the-past`]({{ '/predicates/temporal/is-in-the-past/' | relative_url }}) | Returns true if the date passed as argument is before today. Returns false otherwise. |
| [`is-in-the-past-or-now`]({{ '/predicates/temporal/is-in-the-past-or-now/' | relative_url }}) | Returns true if the dateTime passed as argument is before now. If a Date is passed as argument, it returns true if the date is today or before. Returns false otherwise. |
| [`is-in-the-past-or-today`]({{ '/predicates/temporal/is-in-the-past-or-today/' | relative_url }}) | Returns true if the date passed as argument is today or a date before. If a DateTime is passed as argument, it returns true if the date of this datetime is today or any other date before today. Returns false otherwise. |
| [`is-leap-year`]({{ '/predicates/temporal/is-leap-year/' | relative_url }}) | Returns true if the year of the dateTime value passed as the argument is a leap year. If the argument is not a dateTime but a numeric, returns true if the integer part of this value corresponds to a year that is a leap year. Returns false otherwise. |
| [`is-on-the-day`]({{ '/predicates/temporal/is-on-the-day/' | relative_url }}) | Returns `true` if the argument is of type `DateOnly` or of type `DateTime` but the Time part is set at exactly midnight. Returns `false` otherwise. |
| [`is-on-the-hour`]({{ '/predicates/temporal/is-on-the-hour/' | relative_url }}) | Returns `true` if the argument is of type `DateTime` and the minutes, seconds and milliseconds are all set at `0`. Returns `false` otherwise. |
| [`is-on-the-minute`]({{ '/predicates/temporal/is-on-the-minute/' | relative_url }}) | Returns `true` if the argument is of type `DateTime` and the seconds and milliseconds are all set at `0`. Returns `false` otherwise. |
| [`is-same-instant`]({{ '/predicates/temporal/is-same-instant/' | relative_url }}) | Returns true if the temporal value passed as argument is equal to the temporal value passed as parameter. |
| [`is-today`]({{ '/predicates/temporal/is-today/' | relative_url }}) | Returns true if the date passed as argument is representing the current date. Returns false otherwise. |
| [`is-tomorrow`]({{ '/predicates/temporal/is-tomorrow/' | relative_url }}) | Returns true if the date passed as argument is representing the next date compared to the current date. Returns false otherwise. |
| [`is-weekday`]({{ '/predicates/temporal/is-weekday/' | relative_url }}) | Returns `true` if the date passed as the argument corresponds to the weekday passed as the parameter. Returns `false` otherwise. |
| [`is-weekend`]({{ '/predicates/temporal/is-weekend/' | relative_url }}) | Returns `true` if the date passed as the argument corresponds to a Saturday or a Sunday. Returns `false` otherwise. |
| [`is-within-current-month`]({{ '/predicates/temporal/is-within-current-month/' | relative_url }}) | Returns true if the date passed as argument is part of the same month than the current date. Returns false otherwise. |
| [`is-within-current-week`]({{ '/predicates/temporal/is-within-current-week/' | relative_url }}) | Returns true if the date passed as argument is part of the same week than the current date. A week is starting on Monday and ending on Sunday. Returns false otherwise. |
| [`is-within-current-year`]({{ '/predicates/temporal/is-within-current-year/' | relative_url }}) | Returns true if the date passed as argument is part of the same year than the current date. Returns false otherwise. |
| [`is-within-last-month`]({{ '/predicates/temporal/is-within-last-month/' | relative_url }}) | Returns true if the date passed as argument is part of the month preceding than the current month. Returns false otherwise. |
| [`is-within-last-week`]({{ '/predicates/temporal/is-within-last-week/' | relative_url }}) | Returns true if the date passed as argument is part of the week preceding the current week. A week is starting on Monday and ending on Sunday. Returns false otherwise. |
| [`is-within-last-year`]({{ '/predicates/temporal/is-within-last-year/' | relative_url }}) | Returns true if the date passed as argument is part of the year preceding the current year. Returns false otherwise. |
| [`is-within-next-days`]({{ '/predicates/temporal/is-within-next-days/' | relative_url }}) | Returns true if the date passed as argument is between tomorrow and the count of days after the current date. Returns false otherwise. |
| [`is-within-previous-days`]({{ '/predicates/temporal/is-within-previous-days/' | relative_url }}) | Returns true if the date passed as argument is between the count of days before the current date and yesterday (both included). Returns false otherwise. |
| [`is-within-upcoming-month`]({{ '/predicates/temporal/is-within-upcoming-month/' | relative_url }}) | Returns true if the date passed as argument is part of the month following than the current month. Returns false otherwise. |
| [`is-within-upcoming-week`]({{ '/predicates/temporal/is-within-upcoming-week/' | relative_url }}) | Returns true if the date passed as argument is part of the week following the current week. A week is starting on Monday and ending on Sunday. Returns false otherwise. |
| [`is-within-upcoming-year`]({{ '/predicates/temporal/is-within-upcoming-year/' | relative_url }}) | Returns true if the date passed as argument is part of the year following the current year. Returns false otherwise. |
| [`is-yesterday`]({{ '/predicates/temporal/is-yesterday/' | relative_url }}) | Returns true if the date passed as argument is representing the previous date compared to the current date. Returns false otherwise. |
