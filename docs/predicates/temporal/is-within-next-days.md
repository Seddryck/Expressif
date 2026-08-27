---
layout: docs
title: "is-within-next-days"
parent: "Temporal predicates"
grand_parent: "Predicates library"
nav_order: 280
has_toc: false
permalink: /predicates/temporal/is-within-next-days/
tags:
  - predicates
  - temporal
generated: true
---

```
is-within-next-days(
    count: integer
)
```

Returns true if the date passed as argument is between tomorrow and the count of days after the current date. Returns false otherwise.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `count` | `integer` | Yes | Count of days to move forward. A value of 1 is equivalent to the predicate `tomorrow` and a value of 0 will return false. |





**Kind:** Predicate  
**Scope:** `temporal`  
**Aliases:** `within-next-days`, `dateTime-is-within-next-days`
{: .member-reference }
