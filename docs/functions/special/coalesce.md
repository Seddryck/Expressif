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
| `expressions` | `expression` | Variadic (two or more) | Two or more candidate expressions evaluated from left to right against the same input. |





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
