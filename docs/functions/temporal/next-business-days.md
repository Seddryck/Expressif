---
layout: docs
title: "next-business-days"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 330
has_toc: false
permalink: /functions/temporal/next-business-days/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
next-business-days(
    count: integer
) → date
```

Returns a new date value corresponding to the date passed as the argument, counting forward the business days (being weekdays different of Saturday and Sunday) specified as the parameter. It always returns a business day, as such if the date passed as the argument is a weekend, it considers that this date was the Friday before the argument value.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `count` | `integer` | Yes | The count of business days to move forward. |





**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `temporal-to-next-business-days`, `next-business-day`, `add-business-days`
{: .member-reference }
