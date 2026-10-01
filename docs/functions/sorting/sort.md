---
layout: docs
title: "sort"
parent: "Sorting functions"
grand_parent: "Functions library"
nav_order: 170
has_toc: false
permalink: /functions/sorting/sort/
tags:
  - functions
  - sorting
generated: true
---

```
sort-table<T> →
sort() → array<T>
```

Stably sorts a normalized sort table and returns its original row values.



## Parameters



This function has no parameters.



## Value shape

- **Pipeline input:** `sort-table<T>`
- **Returns:** `array<T>`

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- Cardinality: `preserved` <span class="semantics-info" title="The output contains the same number of elements as the visited input." aria-label="Cardinality definition: The output contains the same number of elements as the visited input.">i</span>
- Dependency: `whole-input` <span class="semantics-info" title="An output depends on the complete visited input." aria-label="Dependency definition: An output depends on the complete visited input.">i</span>
- Ordering: `reordered` <span class="semantics-info" title="The operator deliberately changes relative order." aria-label="Ordering definition: The operator deliberately changes relative order.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.




## Behavior

Rows are compared lexicographically by headers. Null placement is resolved before invoking a comparer; descending direction reverses only non-equal ordering results. Comparers are invoked only for two non-null values and must return a non-null ordering value. Equal keys preserve original row order.



**Kind:** Function  
**Scope:** `sorting`  
**Aliases:** None
{: .member-reference }
