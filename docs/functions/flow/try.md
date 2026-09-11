---
layout: docs
title: "try"
parent: "Flow functions"
grand_parent: "Functions library"
nav_order: 80
has_toc: false
permalink: /functions/flow/try/
tags:
  - functions
  - flow
generated: true
---

```
any →
try(
    ...branches: entry
) → any
```

Returns the first candidate result accepted by its predicate, or the final fallback, or null.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `branches` | `entry` | Variadic (two or more) | Ordered branches with an optional final catch-all fallback. |



## Argument evaluation

- **`branches`:** Branches reuse the original input and stop at the first accepted branch. The output type depends on the selected expression.



## Examples

{% raw %}
```expressif
-5 | try(absolute => greater-than(10), _ => 0) → 0
```
{% endraw %}


**Kind:** Function  
**Scope:** `flow`  
**Aliases:** None
{: .member-reference }
