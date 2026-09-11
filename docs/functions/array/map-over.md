---
layout: docs
title: "map-over"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 70
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



## Argument evaluation

Visits each element of the values argument supplied to this map-over call, in declaration order.

- **`expression`:** For a leading explicit binding, evaluated once per supplied item with T(outer input, ...item arguments); only the supplied item tuple is expanded one level. Following stages consume the preceding result, while their argument expressions (including @_) retain the supplied item as context.
- **`values`:** Evaluated once against the enclosing expression's input, which pipeline stages do not replace.


## Behavior

Prepare the invocation tuple only for a leading `bind("f")`, `rotate | bind("f")`, or `rotate(1) | bind("f")`, including their equivalent tilde forms. Parentheses around an open operation are transparent; an input-bound expression introduces its own boundary. Later bindings consume the preceding stage result without preparing another tuple. The tuple-valued outer input remains one position and nested item tuples are not recursively expanded. Legacy bare-callable argument injection remains available during deprecation; use `f~` for an explicit invocation. Ordinary complete expressions retain their existing input and argument contexts.



## Examples

{% raw %}
```expressif
5 | map-over(subtract~, {10, 11}) → {-5, -6}
20 | map-over(subtract($2), {T(1, 2), T(3, 4)}) → {18, 16}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array`  
**Aliases:** None
{: .member-reference }
