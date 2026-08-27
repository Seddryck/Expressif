---
layout: docs
title: "Special functions"
parent: "Functions library"

nav_order: 50
has_children: true
has_toc: false
permalink: /functions/special-functions/
tags:
  - functions
  - special

generated: true
---

Reference documentation for Expressif functions in the `special` scope.

| Name | Overview |
|:-----|:---------|
| [`any-to-any`]({{ '/functions/special/any-to-any/' | relative_url }}) | Returns `any`. |
| [`coalesce`]({{ '/functions/special/coalesce/' | relative_url }}) | Returns the first non-null result from two or more expressions evaluated from left to right against the same input. Returns `null` when every expression evaluates to `null`. |
| [`coerce-boolean`]({{ '/functions/special/coerce-boolean/' | relative_url }}) | Attempts to convert the input to a boolean value. Returns `null` when the input cannot be converted. |
| [`coerce-date`]({{ '/functions/special/coerce-date/' | relative_url }}) | Attempts to convert the input to a date value. Returns `null` when the input cannot be converted. |
| [`coerce-datetime`]({{ '/functions/special/coerce-datetime/' | relative_url }}) | Attempts to convert the input to a date-time value. Returns `null` when the input cannot be converted. |
| [`coerce-int`]({{ '/functions/special/coerce-int/' | relative_url }}) | Attempts to convert the input to an integer value. Returns `null` when the input cannot be converted without loss. |
| [`coerce-numeric`]({{ '/functions/special/coerce-numeric/' | relative_url }}) | Attempts to convert the input to a numeric value. Returns `null` when the input cannot be converted. |
| [`coerce-text`]({{ '/functions/special/coerce-text/' | relative_url }}) | Attempts to convert the input to a text value. Returns `null` when the input cannot be converted. |
| [`coerce-time`]({{ '/functions/special/coerce-time/' | relative_url }}) | Attempts to convert the input to a time value. Returns `null` when the input cannot be converted. |
| [`neutral`]({{ '/functions/special/neutral/' | relative_url }}) | Returns the argument value. |
| [`null-to-value`]({{ '/functions/special/null-to-value/' | relative_url }}) | Returns the value passed as argument, except if the value is `null` then it returns `value`. |
| [`tuple-at`]({{ '/functions/special/tuple-at/' | relative_url }}) | Returns the tuple field at the specified zero-based position. Returns `null` when the input is not a tuple or the position is out of range. |
| [`tuple-first`]({{ '/functions/special/tuple-first/' | relative_url }}) | Returns the first field of a tuple. Returns `null` when the input is not a tuple. |
| [`tuple-second`]({{ '/functions/special/tuple-second/' | relative_url }}) | Returns the second field of a tuple. Returns `null` when the input is not a tuple. |
| [`value-to-value`]({{ '/functions/special/value-to-value/' | relative_url }}) | Returns `value` except if the argument value is `null` then it returns `null`. |
