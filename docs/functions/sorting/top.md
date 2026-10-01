---
layout: docs
title: "top"
parent: "Sorting functions"
grand_parent: "Functions library"
nav_order: 220
has_toc: false
permalink: /functions/sorting/top/
tags:
  - functions
  - sorting
generated: true
---

```
sort-table<T> →
top(
    count: integer
) → array<T>
```

Returns up to count original values from the first rows in sort table order.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `count` | `integer` | Yes | The maximum number of rows to select. |



## Value shape

- **Pipeline input:** `sort-table<T>`
- **Returns:** `array<T>`

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- Cardinality: `non-increasing` <span class="semantics-info" title="The output contains no more elements than the visited input." aria-label="Cardinality definition: The output contains no more elements than the visited input.">i</span>
- Dependency: `whole-input` <span class="semantics-info" title="An output depends on the complete visited input." aria-label="Dependency definition: An output depends on the complete visited input.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

- **`count`:** Evaluated once in the surrounding evaluation context of this top call; the SortTable supplied as pipeline input is used only for row selection.


## Behavior

Uses the shared SortTable comparator and stable source order to cut boundary ties strictly at count. Results retain table order. Zero count and empty input return an empty array; oversized count returns all rows. Negative count fails with an argument error. Partial selection does not require sorting unselected rows; invalid comparer results fail as for sort.



**Kind:** Function  
**Scope:** `sorting`  
**Aliases:** None
{: .member-reference }
