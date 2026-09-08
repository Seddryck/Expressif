---
layout: docs
title: "map-with"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 140
has_toc: false
permalink: /functions/array/map-with/
tags:
  - functions
  - array
generated: true
---

```
any →
map-with(
    expression: expression,
    values: array
) → array
```

Evaluates an expression once for every supplied value, using that value as the pipeline input and the outer input as its argument. Tuple values remain ordinary pipeline values and are not expanded. Returns `null` when values is not enumerable or is text.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `expression` | `expression` | Yes | Expression evaluated with each supplied value as input and the outer pipeline input as its argument. |
| `values` | `array` | Yes | Values iterated as pipeline inputs in declaration order. |

## Argument evaluation

Visits each value supplied by the values argument.

- **`expression`:** Iterates the values argument, using each supplied value as the expression input and supplying the call's incoming value as the argument to a bare callable.
- **`values`:** Evaluated once against the enclosing expression's input, which pipeline stages do not replace.

## Examples

{% raw %}
```expressif
5 | map-with(subtract, {10, 11}) → {5, 6}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array`  
**Aliases:** None
{: .member-reference }
