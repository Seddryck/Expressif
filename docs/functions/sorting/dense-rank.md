---
layout: docs
title: "dense-rank"
parent: "Sorting functions"
grand_parent: "Functions library"
nav_order: 100
has_toc: false
permalink: /functions/sorting/dense-rank/
tags:
  - functions
  - sorting
generated: true
---

```
sort-table →
dense-rank() → grouping
```

Groups original sort table row values by their one-based dense rank without gaps.



## Parameters



This function has no parameters.





## Behavior

Uses the same stable ordering and adjacent comparer-based tie detection as rank. Each changed key increments the rank by one. Returns original values in stable order and an empty grouping for empty input. Invalid comparer results fail as for sort.



**Kind:** Function  
**Scope:** `sorting`  
**Aliases:** None
{: .member-reference }
