---
layout: docs
title: "switch"
parent: "Flow functions"
grand_parent: "Functions library"
nav_order: 30
has_toc: false
permalink: /functions/flow/switch/
tags:
  - functions
  - flow
generated: true
---

```
any →
switch(
    ...branches: entry
) → any
```

Returns the result of the first branch whose predicate accepts the original input, or the final fallback, or null.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `branches` | `entry` | Variadic (one or more) | Ordered branches with an optional final catch-all fallback. |



## Argument evaluation

- **`branches`:** Branches reuse the original input and stop at the first accepted branch. The output type depends on the selected expression.



## Examples

{% raw %}
```expressif
82 | switch(less-than(50) => "failed", _ => "passed") → "passed"
```
{% endraw %}


**Kind:** Function  
**Scope:** `flow`  
**Aliases:** None
{: .member-reference }
