---
layout: docs
title: "implies"
parent: "Boolean predicates"
grand_parent: "Predicates library"
nav_order: 20
has_toc: false
permalink: /predicates/boolean/implies/
tags:
  - predicates
  - boolean
generated: true
---

```
boolean →
implies(
    expression: boolean
) → boolean
```

Returns logical implication from the Boolean input to a secondary Boolean expression. Returns `true` without evaluating the expression when the input is `false`.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `expression` | `boolean` | Yes | Specifies the secondary Boolean expression evaluated when the input is `true`. |

## Argument evaluation

- **`expression`:** Evaluated only when the incoming value converts to true. References use their enclosing context; open predicate expressions use the current evaluation value.

## Examples

{% raw %}
```expressif
#true | implies(#false) → #false
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `boolean`  
**Aliases:** None
{: .member-reference }
