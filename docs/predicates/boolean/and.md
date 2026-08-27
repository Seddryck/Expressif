---
layout: docs
title: "and"
parent: "Boolean predicates"
grand_parent: "Predicates library"
nav_order: 10
has_toc: false
permalink: /predicates/boolean/and/
tags:
  - predicates
  - boolean
generated: true
---

```
and(
    expression: any
)
```

Returns the logical conjunction of the Boolean-converted input and a secondary predicate expression. Null-like and unconvertible values convert to `false`; zero converts to `false`; nonzero numbers and recognized true text convert to `true`. Evaluates the secondary expression only when the converted input is `true`.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `expression` | `any` | Yes | Specifies the secondary predicate expression evaluated when the converted input is `true`. |





**Kind:** Predicate  
**Scope:** `boolean`  
**Aliases:** None
{: .member-reference }
