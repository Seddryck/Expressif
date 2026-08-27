---
layout: docs
title: "is-before-or-same-instant"
parent: "Temporal predicates"
grand_parent: "Predicates library"
nav_order: 40
has_toc: false
permalink: /predicates/temporal/is-before-or-same-instant/
tags:
  - predicates
  - temporal
generated: true
---

```
is-before-or-same-instant(
    reference: date-time
)
```

Returns true if the temporal value passed as argument is chronologically before the temporal value passed as parameter or if the two values represent the same instant . Returns `false` otherwise.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `reference` | `date-time` | Yes | A temporal value to compare to the argument. |





**Kind:** Predicate  
**Scope:** `temporal`  
**Aliases:** `before-or-same-instant`, `dateTime-is-before-or-same-instant`
{: .member-reference }
