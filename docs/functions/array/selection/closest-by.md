---
layout: docs
title: "closest-by"
parent: "Selection functions"
grand_parent: "Array functions"
nav_order: 10
has_toc: false
permalink: /functions/array/selection/closest-by/
tags:
  - functions
  - array/selection
generated: true
---

```
array →
closest-by(
    expression: expression,
    target: numeric
) → any
```

Returns the original source element whose expression result is nearest to the numeric target. Preserves the first match on ties and skips null criteria. Returns `null` for empty arrays, all-null criteria, or non-array input.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `expression` | `expression` | Yes | Expression evaluated once for each source element to obtain its comparison criterion. |
| `target` | `numeric` | Yes | Numeric value against which criterion distances are compared. |

## Argument evaluation

Visits each element of the array entering this call.

- **`expression`:** Evaluated once per visited element, with that element as its context.
- **`target`:** Evaluated once in the enclosing context. The resulting target is reused for all array elements.

## Behavior

The output preserves the selected source element and its runtime type; it depends on the input element type, not the expression result type. Traverses the source once in order with constant auxiliary storage. Min-by and max-by compare numeric values across numeric types, text using ordinal ordering, and other same-type comparable scalar values; incompatible or non-comparable criteria fail. Closest-by requires numeric criteria and a numeric target. Expressions execute once per element, including after an exact match.



## Examples

{% raw %}
```expressif
{{bar := 10}, {bar := 30}} | closest-by(.bar, 32) → {bar := 30}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/selection`  
**Aliases:** None
{: .member-reference }
