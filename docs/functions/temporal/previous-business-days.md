---
layout: docs
title: "previous-business-days"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 390
has_toc: false
permalink: /functions/temporal/previous-business-days/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
previous-business-days(
    count: integer
) → date
```

Returns a new date value corresponding to the date passed as the argument, counting backward the business days (being weekdays different of Saturday and Sunday) specified as the parameter. It always returns a business day, as such if the date passed as the argument is a weekend, it considers that this date was the Friday before the argument value.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `count` | `integer` | Yes | The count of business days to move forward. |





**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `temporal-to-previous-business-days`, `previous-business-day`, `subtract-business-days`
{: .member-reference }
