---
layout: docs
title: "min-by"
parent: "Selection functions"
grand_parent: "Array functions"
nav_order: 50
has_toc: false
permalink: /functions/array/selection/min-by/
tags:
  - functions
  - array/selection
generated: true
---

```
array →
min-by(
    expression: expression
) → any
```

Returns the original source element whose expression result is smallest. Preserves the first match on ties and skips null criteria. Returns `null` for empty arrays, all-null criteria, or non-array input.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `expression` | `expression` | Yes | Expression evaluated once for each source element to obtain its comparison criterion. |

## Argument evaluation

Visits each element of the array entering this call.

- **`expression`:** Evaluated once per visited element, with that element as its context.

## Behavior

The output preserves the selected source element and its runtime type; it depends on the input element type, not the expression result type. Traverses the source once in order with constant auxiliary storage. Min-by and max-by compare numeric values across numeric types, text using ordinal ordering, and other same-type comparable scalar values; incompatible or non-comparable criteria fail. Closest-by requires numeric criteria and a numeric target. Expressions execute once per element, including after an exact match.



## Examples

{% raw %}
```expressif
{ -8, -3, 3 } | min-by(absolute) → -3
{{bar := 10}, {bar := 30}} | min-by(.bar) → {bar := 10}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/selection`  
**Aliases:** None
{: .member-reference }
