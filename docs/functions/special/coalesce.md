---
layout: docs
title: "coalesce"
parent: "Special functions"
grand_parent: "Functions library"
nav_order: 20
has_toc: false
permalink: /functions/special/coalesce/
tags:
  - functions
  - special
generated: true
---

```
any →
coalesce(
    ...expressions: expression
) → any
```

Returns the first non-null result from two or more expressions evaluated from left to right against the same input. Returns `null` when every expression evaluates to `null`.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `expressions` | `expression` | Variadic (two or more); no spread | Two or more candidate expressions evaluated from left to right against the same input. |



## Value shape

- **Pipeline input:** `T`
- **Returns:** `nullable<U>`
- **`expressions`:** Receives `T` and returns `U`.
- **Combination:** When multiple values are supplied, their output types are combined as a union.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.




## Argument evaluation

- **`expressions`:** Candidates are evaluated against the same incoming value, from left to right, until one produces a non-null result. Later candidates are skipped.



## Examples

{% raw %}
```expressif
#null | coalesce(#null, 42) → 42
```
{% endraw %}


**Kind:** Function  
**Scope:** `special`  
**Aliases:** None
{: .member-reference }
