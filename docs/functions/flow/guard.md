---
layout: docs
title: "guard"
parent: "Flow functions"
grand_parent: "Functions library"
nav_order: 10
has_toc: false
permalink: /functions/flow/guard/
tags:
  - functions
  - flow
generated: true
---

```
any →
guard(
    expression: expression
) → any
```

Evaluates an expression only when the current input is directly compatible with its entry contract; otherwise, returns the original input unchanged.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `expression` | `expression` | Yes | Expression evaluated when its entry contract directly accepts the current input. |





## Behavior

`guard` checks only whether the current value can enter the supplied expression without coercion. If entry is compatible, the complete expression runs normally, including ordinary coercion between later stages. If entry would require coercion, the expression is skipped and the original value is returned unchanged. The `*expression` syntax is shorthand for `guard(expression)`.



## Examples

{% raw %}
```expressif
"  Bob  " | *trim → "Bob"
"42" | *trim → "42"
"5" | *add(1) → "5"
42 | *trim → 42
" Bob " | *(trim | append-space) → "Bob "
```
{% endraw %}


**Kind:** Function  
**Scope:** `flow`  
**Aliases:** None
{: .member-reference }
