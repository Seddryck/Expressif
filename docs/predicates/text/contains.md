---
layout: docs
title: "contains"
parent: "Text predicates"
grand_parent: "Predicates library"
nav_order: 10
has_toc: false
permalink: /predicates/text/contains/
tags:
  - predicates
  - text
generated: true
---

```
contains(
    reference: text,
    comparer?: any
)
```

Returns `true` if the value passed as argument contains, anywhere in the string, the text value passed as parameter. Returns `false` otherwise.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `reference` | `text` | Yes | A string to be compared to the argument value. |
| `comparer` | `any` | No | A definition of the parameters of the comparison (case-sensitivity, culture-sensitivity). |





**Kind:** Predicate  
**Scope:** `text`  
**Aliases:** `text-contains`
{: .member-reference }
