---
layout: docs
title: "compare"
parent: "Sorting functions"
grand_parent: "Functions library"
nav_order: 20
has_toc: false
permalink: /functions/sorting/compare/
tags:
  - functions
  - sorting
generated: true
---

```
any →
compare(
    right: any,
    type?: type
) → ordering
```

Compares two values in a selected supported domain and returns their relative ordering.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `right` | `any` | Yes | The value compared with the input value. |
| `type` | `type` | No | The comparison domain; defaults to :text. |





## Behavior

:text routes to ordinal comparison; :integer, :decimal, and :numeric route to numeric comparison; :date, :time, and :datetime route to their temporal comparisons. Unsupported domains fail clearly. Returns #null when either value is null or cannot be coerced to the selected domain.



**Kind:** Function  
**Scope:** `sorting`  
**Aliases:** None
{: .member-reference }
