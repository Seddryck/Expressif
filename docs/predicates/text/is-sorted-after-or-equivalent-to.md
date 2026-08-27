---
layout: docs
title: "is-sorted-after-or-equivalent-to"
parent: "Text predicates"
grand_parent: "Predicates library"
nav_order: 90
has_toc: false
permalink: /predicates/text/is-sorted-after-or-equivalent-to/
tags:
  - predicates
  - text
generated: true
---

```
is-sorted-after-or-equivalent-to(
    reference: text,
    comparer?: any
)
```

Compare the text value passed as argument and the text value passed as parameter and returns `true` if argument value is alphabetically sorted after the parameter value or if the two values are equal. By default the comparison is agnostic of the culture and case-insensitive.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `reference` | `text` | Yes | A string to be compared to the argument value. |
| `comparer` | `any` | No | A definition of the parameters of the comparison (case-sensitivity, culture-sensitivity). |





**Kind:** Predicate  
**Scope:** `text`  
**Aliases:** `sorted-after-or-equivalent-to`, `text-is-sorted-after-or-equivalent-to`
{: .member-reference }
