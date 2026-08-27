---
layout: docs
title: "is-before"
parent: "Temporal predicates"
grand_parent: "Predicates library"
nav_order: 30
has_toc: false
permalink: /predicates/temporal/is-before/
tags:
  - predicates
  - temporal
generated: true
---

```
is-before(
    reference: date-time
)
```

Returns true if the temporal value passed as argument is chronologically before the temporal value passed as parameter. Returns `false` otherwise.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `reference` | `date-time` | Yes | A temporal value to compare to the argument |





**Kind:** Predicate  
**Scope:** `temporal`  
**Aliases:** `before`, `dateTime-is-before`
{: .member-reference }
