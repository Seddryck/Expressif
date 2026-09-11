---
layout: docs
title: "map-with"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 70
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

Visits each element of the values argument supplied to this map-with call, in declaration order.

- **`expression`:** For a leading explicit binding, evaluated once per supplied item with T(outer input, item), preserving both tuple-valued positions. Following stages consume the preceding result, while their argument expressions (including @_) retain the supplied item as context.
- **`values`:** Evaluated once against the enclosing expression's input, which pipeline stages do not replace.


## Behavior

Prepare the invocation tuple only for a leading `bind("f")`, `rotate | bind("f")`, or `rotate(1) | bind("f")`, including their equivalent tilde forms. Parentheses around an open operation are transparent; an input-bound expression introduces its own boundary. Later bindings consume the preceding stage result without preparing another tuple. The supplied item and outer input each remain one position, even when either is a tuple. Legacy bare-callable argument injection remains available during deprecation; use `~f` for an explicit invocation. Ordinary complete expressions retain their existing input and argument contexts.



## Examples

{% raw %}
```expressif
5 | map-with(~subtract, {10, 11}) → {5, 6}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array`  
**Aliases:** None
{: .member-reference }
