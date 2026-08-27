---
layout: docs
title: "starts-with"
parent: "Text predicates"
grand_parent: "Predicates library"
nav_order: 180
has_toc: false
permalink: /predicates/text/starts-with/
tags:
  - predicates
  - text
generated: true
---

```
starts-with(
    reference: text,
    comparer?: any
)
```

Returns `true` if the value passed as argument starts with the text value passed as parameter. Returns `false` otherwise.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `reference` | `text` | Yes | A string to be compared to the argument value.. |
| `comparer` | `any` | No | A definition of the parameters of the comparison (case-sensitivity, culture-sensitivity). |





**Kind:** Predicate  
**Scope:** `text`  
**Aliases:** `text-starts-with`
{: .member-reference }
