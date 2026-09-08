---
layout: docs
title: "add"
parent: "Arithmetic functions"
grand_parent: "Numeric functions"
nav_order: 20
has_toc: false
permalink: /functions/numeric/arithmetic/add/
tags:
  - functions
  - numeric/arithmetic
generated: true
---

```
numeric →
add(
    value: numeric,
    times: integer = 1
) → numeric
```

Returns the sum of the input value and the parameter value.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `value` | `numeric` | Yes | The value to add to the input value. |
| `times` | `integer` | No | Number of times the addition is applied. Defaults to `1`. |



## Argument evaluation

- **`value`:** Evaluated once against `enclosing` → `whole`.
- **`times`:** Evaluated once against `enclosing` → `whole`.


## Behavior

**Argument form — `value` and `times`:** Value expressions: literals, references, or expression pipelines, such as `5`, `.bonus`, or `5 | multiply(2)`. `times` defaults to `1`; the result is `input + value × times`.

**Enclosing context:** Both arguments retain the surrounding expression context. Field references read its contextual record, independently of the number entering `add`. For example, `{price:=10, bonus:=3} | .price | add(.bonus)` returns `13`: the pipeline input to `add` is `10`, while `.bonus` reads `3` from the surrounding record.



## Examples

{% raw %}
```expressif
10 | add(5)      → 15
10 | add(5, 2)   → 20
```
{% endraw %}


**Kind:** Function  
**Scope:** `numeric/arithmetic`  
**Aliases:** `numeric-to-add`
{: .member-reference }
