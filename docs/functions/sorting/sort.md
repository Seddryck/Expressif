---
layout: docs
title: "sort"
parent: "Sorting functions"
grand_parent: "Functions library"
nav_order: 160
has_toc: false
permalink: /functions/sorting/sort/
tags:
  - functions
  - sorting
generated: true
---

```
sort-table →
sort() → array
```

Stably sorts a normalized sort table and returns its original row values.



## Parameters



This function has no parameters.





## Behavior

Rows are compared lexicographically by headers. Null placement is resolved before invoking a comparer; descending direction reverses only non-equal ordering results. Comparers are invoked only for two non-null values and must return a non-null ordering value. Equal keys preserve original row order.



**Kind:** Function  
**Scope:** `sorting`  
**Aliases:** None
{: .member-reference }
