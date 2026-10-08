---
layout: docs
title: "rank"
parent: "Sorting functions"
grand_parent: "Functions library"
nav_order: 150
has_toc: false
permalink: /functions/sorting/rank/
tags:
  - functions
  - sorting
generated: true
---

```
sort-table<T> →
rank() → grouping<integer, T>
```

Groups original sort table row values by their one-based SQL rank.



## Parameters



This function has no parameters.



## Value shape

- Pipeline input: `sort-table<T>`
- Returns: `grouping<integer, T>`

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- Cardinality: `partitioned` <span class="semantics-info" title="Visited inputs are reorganized into groups or partitions." aria-label="Cardinality definition: Visited inputs are reorganized into groups or partitions.">i</span>
- Dependency: `whole-input` <span class="semantics-info" title="An output depends on the complete visited input." aria-label="Dependency definition: An output depends on the complete visited input.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.




## Behavior

Rows use the same stable lexicographic ordering and comparer failure contract as sort. Adjacent ordered keys that compare equal share a rank; each changed key starts a rank equal to its one-based row position, leaving gaps after ties. Groups appear in ascending rank order and retain stable original-value order. Empty input returns an empty grouping. Ranking assumes a coherent comparer relation with transitive comparison equality; invalid relations are not normalized.



**Kind:** Function  
**Scope:** `sorting`  
**Aliases:** None
{: .member-reference }
