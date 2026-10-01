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
sort-table<T> →
top-with-ties(
    count: integer
) → array<T>
```

Returns the first count rows and all comparer-equal boundary ties in sort table order.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `count` | `integer` | Yes | The requested row count before including boundary ties. |



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

- **`count`:** Evaluated once in the surrounding evaluation context of this top-with-ties call; the SortTable supplied as pipeline input is used only for row selection.


## Behavior

Uses the shared full SortTable comparator to include every row tied with the selection boundary. Results retain table order and stable source order within ties. Zero count and empty input return an empty array; oversized count returns all rows. Negative count fails with an argument error. Uses partial selection followed by a boundary scan and ordering of selected rows; invalid comparer results fail as for sort.



**Kind:** Function  
**Scope:** `sorting`  
**Aliases:** None
{: .member-reference }
