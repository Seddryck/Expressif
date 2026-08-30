---
layout: docs
title: "is-equal-to"
parent: "Numeric predicates"
grand_parent: "Predicates library"
nav_order: 10
has_toc: false
permalink: /predicates/numeric/is-equal-to/
tags:
  - predicates
  - numeric
generated: true
---

```
is-equal-to(
    reference: numeric
)
```

Returns true if the numeric value passed as argument is equal to the numeric value passed as parameter.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `reference` | `numeric` | Yes | A numeric value to compare to the argument. |






## Examples

{% raw %}
```expressif
10 | is-equal-to(5) → #false
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `numeric`  
**Aliases:** `equal-to`, `numeric-is-equal-to`
{: .member-reference }
