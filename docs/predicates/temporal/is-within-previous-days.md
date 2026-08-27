---
layout: docs
title: "is-within-previous-days"
parent: "Temporal predicates"
grand_parent: "Predicates library"
nav_order: 290
has_toc: false
permalink: /predicates/temporal/is-within-previous-days/
tags:
  - predicates
  - temporal
generated: true
---

```
is-within-previous-days(
    count: integer
)
```

Returns true if the date passed as argument is between the count of days before the current date and yesterday (both included). Returns false otherwise.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `count` | `integer` | Yes | Count of days to move backward. A value of 1 is equivalent to the predicate `yesterday` and a value of 0 will return false. |





**Kind:** Predicate  
**Scope:** `temporal`  
**Aliases:** `within-previous-days`, `dateTime-is-within-previous-days`
{: .member-reference }
