---
layout: docs
title: "rank-by"
parent: "Sorting functions"
grand_parent: "Functions library"
nav_order: 160
has_toc: false
permalink: /functions/sorting/rank-by/
tags:
  - functions
  - sorting
generated: true
---

```
array →
rank-by(
    ...criteria: expression
) → grouping
```

Groups array elements by their one-based SQL rank using typed criteria.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `criteria` | `expression` | Variadic (one or more) | Criteria in the form expression -> :type, optionally followed by direction and null-placement modifiers. |



## Argument evaluation

Visits each element of the array supplied as pipeline input to this rank-by call.

- **`criteria`:** Evaluated once per visited element in declaration order, with that element as context; .field reads its field and $0 reads its first tuple component.


## Behavior

Builds a SortTable using the same criterion lowering as sort-by and delegates to rank. Comparer-equal keys share a group. Empty input returns an empty grouping. Invalid coercions become null keys.



**Kind:** Function  
**Scope:** `sorting`  
**Aliases:** None
{: .member-reference }
