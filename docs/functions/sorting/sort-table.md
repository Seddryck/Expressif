---
layout: docs
title: "sort-table"
parent: "Sorting functions"
grand_parent: "Functions library"
nav_order: 190
has_toc: false
permalink: /functions/sorting/sort-table/
tags:
  - functions
  - sorting
generated: true
---

```
array →
sort-table() → sort-table
```

Normalizes pairs of sort keys and original values into shared headers and data rows.



## Parameters



This function has no parameters.





## Behavior

Every input item must be a pair whose key is a SortKey. All keys must have equal arity and matching comparer identity, direction, and null policy at each position. Header metadata is hoisted once; rows retain key values and original values. Empty input produces a headerless, rowless SortTable. No sorting is performed.



**Kind:** Function  
**Scope:** `sorting`  
**Aliases:** None
{: .member-reference }
