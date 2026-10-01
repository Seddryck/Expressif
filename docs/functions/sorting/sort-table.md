---
layout: docs
title: "sort-table"
parent: "Sorting functions"
grand_parent: "Functions library"
nav_order: 200
has_toc: false
permalink: /functions/sorting/sort-table/
tags:
  - functions
  - sorting
generated: true
---

```
array<pair<sort-key, T>> →
sort-table() → sort-table<T>
```

Normalizes pairs of sort keys and original values into shared headers and data rows.



## Parameters



This function has no parameters.



## Value shape

- Pipeline input: `array<pair<sort-key, T>>`
- Returns: `sort-table<T>`
- Nullability: The result is nullable when the pipeline input is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.






## Behavior

Every input item must be a pair whose key is a SortKey. All keys must have equal arity and matching comparer identity, direction, and null policy at each position. Header metadata is hoisted once; rows retain key values and original values. Empty input produces a headerless, rowless SortTable. No sorting is performed.



**Kind:** Function  
**Scope:** `sorting`  
**Aliases:** None
{: .member-reference }
