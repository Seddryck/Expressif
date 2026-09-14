---
layout: docs
title: "sort-key"
parent: "Sorting functions"
grand_parent: "Functions library"
nav_order: 130
has_toc: false
permalink: /functions/sorting/sort-key/
tags:
  - functions
  - sorting
generated: true
---

```
any →
sort-key(
    ...values: sort-term
) → sort-key
```

Creates a non-empty ordered sort key from one or more sort terms.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `values` | `sort-term` | Variadic (one or more) | One or more sort terms in lexicographic comparison order. |





## Behavior

Every value must be a SortTerm. Declaration order is significant, nested SortKey values are rejected, and no implicit flattening occurs.



**Kind:** Function  
**Scope:** `sorting`  
**Aliases:** None
{: .member-reference }
