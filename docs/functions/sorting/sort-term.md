---
layout: docs
title: "sort-term"
parent: "Sorting functions"
grand_parent: "Functions library"
nav_order: 150
has_toc: false
permalink: /functions/sorting/sort-term/
tags:
  - functions
  - sorting
generated: true
---

```
any →
sort-term(
    value: any,
    comparer: expression
) → sort-term
```

Creates a sort term from a value and a tuple-bound ordering comparer.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `value` | `any` | Yes | The value to compare. |
| `comparer` | `expression` | Yes | A tuple-bound callable reference that returns an ordering value. |





## Behavior

Creates SortTerm(value, comparer, #true, #false), corresponding to ascending order with nulls last. The comparer is retained as a callable reference and is not invoked during construction.



**Kind:** Function  
**Scope:** `sorting`  
**Aliases:** None
{: .member-reference }
