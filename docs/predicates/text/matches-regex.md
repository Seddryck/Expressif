---
layout: docs
title: "matches-regex"
parent: "Text predicates"
grand_parent: "Predicates library"
nav_order: 160
has_toc: false
permalink: /predicates/text/matches-regex/
tags:
  - predicates
  - text
generated: true
---

```
matches-regex(
    regex: text,
    comparer?: any
)
```

Returns `true` if the value passed as argument validate the regex passed as parameter. Returns `false` otherwise.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `regex` | `text` | Yes | A string to be compared to the argument value. |
| `comparer` | `any` | No | A definition of the parameters of the comparison (case-sensitivity, culture-sensitivity). |





**Kind:** Predicate  
**Scope:** `text`  
**Aliases:** `text-matches-regex`
{: .member-reference }
