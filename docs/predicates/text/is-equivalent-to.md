---
layout: docs
title: "is-equivalent-to"
parent: "Text predicates"
grand_parent: "Predicates library"
nav_order: 60
has_toc: false
permalink: /predicates/text/is-equivalent-to/
tags:
  - predicates
  - text
generated: true
---

```
is-equivalent-to(
    reference: text,
    comparer?: any
)
```

Compare the text value passed as argument and the text value passed as parameter and returns `true` if they are equal. By default the comparison is agnostic of the culture and case-insensitive.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `reference` | `text` | Yes | A string to be compared to the argument value. |
| `comparer` | `any` | No | A definition of the parameters of the comparison (case-sensitivity, culture-sensitivity).. |





**Kind:** Predicate  
**Scope:** `text`  
**Aliases:** `equivalent-to`, `text-is-equivalent-to`
{: .member-reference }
