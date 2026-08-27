---
layout: docs
title: "is-contained-in"
parent: "Temporal predicates"
grand_parent: "Predicates library"
nav_order: 60
has_toc: false
permalink: /predicates/temporal/is-contained-in/
tags:
  - predicates
  - temporal
generated: true
---

```
is-contained-in(
    interval: any
)
```

Returns true if the temporal value passed as argument is between the lower bound and the upper bound defined in the interval. Returns `false` otherwise.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `interval` | `any` | Yes | A temporal interval to compare to the argument. |





**Kind:** Predicate  
**Scope:** `temporal`  
**Aliases:** `contained-in`, `dateTime-is-contained-in`
{: .member-reference }
