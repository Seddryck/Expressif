---
layout: docs
title: "rank"
parent: "Sorting functions"
grand_parent: "Functions library"
nav_order: 130
has_toc: false
permalink: /functions/sorting/rank/
tags:
  - functions
  - sorting
generated: true
---

```
sort-table →
rank() → grouping
```

Groups original sort table row values by their one-based SQL rank.



## Parameters



This function has no parameters.





## Behavior

Rows use the same stable lexicographic ordering and comparer failure contract as sort. Adjacent ordered keys that compare equal share a rank; each changed key starts a rank equal to its one-based row position, leaving gaps after ties. Groups appear in ascending rank order and retain stable original-value order. Empty input returns an empty grouping. Ranking assumes a coherent comparer relation with transitive comparison equality; invalid relations are not normalized.



**Kind:** Function  
**Scope:** `sorting`  
**Aliases:** None
{: .member-reference }
