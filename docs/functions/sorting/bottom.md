---
layout: docs
title: "bottom"
parent: "Sorting functions"
grand_parent: "Functions library"
nav_order: 20
has_toc: false
permalink: /functions/sorting/bottom/
tags:
  - functions
  - sorting
generated: true
---

```
sort-table →
bottom(
    count: integer
) → array
```

Returns up to count original values from the last rows in sort table order.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `count` | `integer` | Yes | The maximum number of rows to select. |



## Argument evaluation

- **`count`:** Evaluated once in the surrounding evaluation context of this bottom call; the SortTable supplied as pipeline input is used only for row selection.


## Behavior

Uses the shared SortTable comparator and stable source order to cut boundary ties strictly at count. Results retain table order. Zero count and empty input return an empty array; oversized count returns all rows. Negative count fails with an argument error. Partial selection does not require sorting unselected rows; invalid comparer results fail as for sort.



**Kind:** Function  
**Scope:** `sorting`  
**Aliases:** None
{: .member-reference }
