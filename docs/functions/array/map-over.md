---
layout: docs
title: "map-over"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 130
has_toc: false
permalink: /functions/array/map-over/
tags:
  - functions
  - array
generated: true
---

```
any →
map-over(
    expression: expression,
    values: array
) → array
```

Evaluates an expression once for every supplied value while preserving the pipeline input as the expression input. Tuple values are expanded into positional arguments for a bare callable. Returns `null` when values is not enumerable or is text.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `expression` | `expression` | Yes | Expression evaluated with the outer pipeline input and each supplied value as its argument context. |
| `values` | `array` | Yes | Values iterated as argument contexts in declaration order. |






## Examples

{% raw %}
```expressif
5 | map-over(subtract, {10, 11}) → {-5, -6}
20 | map-over(subtract($2), {T(1, 2), T(3, 4)}) → {18, 16}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array`  
**Aliases:** None
{: .member-reference }
