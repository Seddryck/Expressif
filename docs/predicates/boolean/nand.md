---
layout: docs
title: "nand"
parent: "Boolean predicates"
grand_parent: "Predicates library"
nav_order: 90
has_toc: false
permalink: /predicates/boolean/nand/
tags:
  - predicates
  - boolean
generated: true
---

```
boolean →
nand(
    expression: boolean
) → boolean
```

Returns the negation of the logical conjunction of the Boolean input and a secondary Boolean expression. Evaluates the secondary expression only when the input is `true`.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `expression` | `boolean` | Yes | Specifies the secondary Boolean expression evaluated when the input is `true`. |

## Argument evaluation

- **`expression`:** Evaluated only when the incoming value converts to true. References use their enclosing context; open predicate expressions use the current evaluation value.

## Examples

{% raw %}
```expressif
#true | nand(#true) → #false
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `boolean`  
**Aliases:** None
{: .member-reference }
