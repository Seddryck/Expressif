---
layout: docs
title: "is-after"
parent: "Temporal predicates"
grand_parent: "Predicates library"
nav_order: 10
has_toc: false
permalink: /predicates/temporal/is-after/
tags:
  - predicates
  - temporal
generated: true
---

```
is-after(
    reference: date-time
)
```

Returns true if the temporal value passed as argument is chronologically after the temporal value passed as parameter. Returns `false` otherwise.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `reference` | `date-time` | Yes | A temporal value to compare to the argument. |





**Kind:** Predicate  
**Scope:** `temporal`  
**Aliases:** `after`, `dateTime-is-after`
{: .member-reference }
