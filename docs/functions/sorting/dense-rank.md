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
sort-table<T> →
dense-rank() → grouping<integer, T>
```

Groups original sort table row values by their one-based dense rank without gaps.



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

Uses the same stable ordering and adjacent comparer-based tie detection as rank. Each changed key increments the rank by one. Returns original values in stable order and an empty grouping for empty input. Invalid comparer results fail as for sort.



**Kind:** Function  
**Scope:** `sorting`  
**Aliases:** None
{: .member-reference }
