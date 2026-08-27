---
layout: docs
title: "xor"
parent: "Boolean predicates"
grand_parent: "Predicates library"
nav_order: 90
has_toc: false
permalink: /predicates/boolean/xor/
tags:
  - predicates
  - boolean
generated: true
---

```
xor(
    expression: any
)
```

Returns `true` when exactly one of the Boolean-converted input and a secondary predicate expression evaluates to `true`. Null-like and unconvertible values convert to `false`; zero converts to `false`; nonzero numbers and recognized true text convert to `true`. Always evaluates the secondary expression.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `expression` | `any` | Yes | Specifies the secondary predicate expression evaluated after the input. |





**Kind:** Predicate  
**Scope:** `boolean`  
**Aliases:** None
{: .member-reference }
