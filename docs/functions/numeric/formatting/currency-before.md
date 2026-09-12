---
layout: docs
title: "currency-before"
parent: "Formatting functions"
grand_parent: "Numeric functions"
nav_order: 20
has_toc: false
permalink: /functions/numeric/formatting/currency-before/
tags:
  - functions
  - numeric/formatting
generated: true
---

```
numeric →
currency-before(
    symbol: text,
    decimals?: integer,
    separator?: text,
    grouping?: text,
    negative?: text
) → text
```

Formats a numeric value as currency with the symbol before the number. Rounds midpoint values away from zero and returns null for null input or invalid formatting options.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `symbol` | `text` | Yes | Currency symbol placed next to the formatted number. |
| `decimals` | `integer` | No | Number of decimal places from 0 to 28; defaults to 2. |
| `separator` | `text` | No | Nonempty decimal separator; defaults to a point. |
| `grouping` | `text` | No | Thousands separator; defaults to a comma. An empty string disables grouping. |
| `negative` | `text` | No | One character prepended to negative amounts, or two characters enclosing them; defaults to a minus sign. |



## Argument evaluation

- **`symbol`:** Evaluated once, in parameter order, for a numeric input. Expressions retain the enclosing context; field references read that context rather than the number entering this call.
- **`decimals`:** Evaluated once, in parameter order, for a numeric input. Expressions retain the enclosing context; field references read that context rather than the number entering this call.
- **`separator`:** Evaluated once, in parameter order, for a numeric input. Expressions retain the enclosing context; field references read that context rather than the number entering this call.
- **`grouping`:** Evaluated once, in parameter order, for a numeric input. Expressions retain the enclosing context; field references read that context rather than the number entering this call.
- **`negative`:** Evaluated once, in parameter order, for a numeric input. Expressions retain the enclosing context; field references read that context rather than the number entering this call.



## Examples

{% raw %}
```expressif
123.4567 | numeric-to-currency-before("$") → "$123.46"
```
{% endraw %}


**Kind:** Function  
**Scope:** `numeric/formatting`  
**Aliases:** `numeric-to-currency-before`
{: .member-reference }
