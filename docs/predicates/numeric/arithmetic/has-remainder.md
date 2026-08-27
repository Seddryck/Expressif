---
layout: docs
title: "has-remainder"
parent: "Arithmetic predicates"
grand_parent: "Numeric predicates"
nav_order: 10
has_toc: false
permalink: /predicates/numeric/arithmetic/has-remainder/
tags:
  - predicates
  - numeric/arithmetic
generated: true
---

```
has-remainder(
    modulus: numeric,
    remainder: numeric
)
```

Returns `true` if the division of the numeric value passed as argument by the modulus provided as parameter value is equal to the required remainder. Returns `false` otherwise.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `modulus` | `numeric` | Yes | An integer value used as the modulus. |
| `remainder` | `numeric` | Yes | An integer value defined as the expected reminder. |





**Kind:** Predicate  
**Scope:** `numeric/arithmetic`  
**Aliases:** `modulo`, `numeric-is-modulo`
{: .member-reference }
