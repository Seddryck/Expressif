---
layout: docs
title: "top-with-ties"
parent: "Sorting functions"
grand_parent: "Functions library"
nav_order: 230
has_toc: false
permalink: /functions/sorting/top-with-ties/
tags:
  - functions
  - sorting
generated: true
---

```
sort-table →
top-with-ties(
    count: integer
) → array
```

Returns the first count rows and all comparer-equal boundary ties in sort table order.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `count` | `integer` | Yes | The requested row count before including boundary ties. |



## Argument evaluation

- **`count`:** Evaluated once in the surrounding evaluation context of this top-with-ties call; the SortTable supplied as pipeline input is used only for row selection.


## Behavior

Uses the shared full SortTable comparator to include every row tied with the selection boundary. Results retain table order and stable source order within ties. Zero count and empty input return an empty array; oversized count returns all rows. Negative count fails with an argument error. Uses partial selection followed by a boundary scan and ordering of selected rows; invalid comparer results fail as for sort.



**Kind:** Function  
**Scope:** `sorting`  
**Aliases:** None
{: .member-reference }
