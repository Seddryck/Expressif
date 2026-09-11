---
layout: docs
title: "bind"
parent: "Tuple functions"
grand_parent: "Functions library"
nav_order: 20
has_toc: false
permalink: /functions/tuple/bind/
tags:
  - functions
  - tuple
generated: true
---

```
tuple →
bind(
    function: text
) → any
```

Invokes a named callable using the first tuple position as pipeline input and the remaining positions as already-evaluated argument values.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `function` | `text` | Yes | Names the callable to invoke. |



## Argument evaluation

- **`function`:** Evaluated once per invocation in the enclosing argument context of this bind call, before invoking the selected callable. The tuple supplied as pipeline input does not replace that context.


## Behavior

The tuple entering this bind call supplies the target input at position zero and explicit argument values in their original order. Values are never reinterpreted as expressions. Eligible signatures accept values, including optional and variadic arguments; expression-taking signatures require a compatible adapter. Unknown or ineligible targets, empty or non-tuple input, invalid arity and incompatible values produce distinct binding diagnostics. Target output determines the result type. `f~` is equivalent to `bind("f")`; `~f` is equivalent to `rotate | bind("f")`, moving the last item to the front without reversing the remaining arguments. Positive rotation offsets move right; the omitted offset is 1, so rotate(1) | bind("f") is also equivalent to ~f.



## Examples

{% raw %}
```expressif
T(120, 135) | bind("subtract") â†’ -15
T(120, 135) | ~subtract â†’ 15
```
{% endraw %}


**Kind:** Function  
**Scope:** `tuple`  
**Aliases:** None
{: .member-reference }
