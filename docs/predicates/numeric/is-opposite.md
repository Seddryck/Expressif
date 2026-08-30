---
layout: docs
title: "is-opposite"
parent: "Numeric predicates"
grand_parent: "Predicates library"
nav_order: 90
has_toc: false
permalink: /predicates/numeric/is-opposite/
tags:
  - predicates
  - numeric
generated: true
---

```
is-opposite(
    reference: numeric
)
```

Returns true if the numeric value passed as argument additive inverse of the numeric value passed as parameter. Returns `false` otherwise.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `reference` | `numeric` | Yes | A numeric value to compare to the argument. |






## Examples

{% raw %}
```expressif
10 | is-opposite(5) → #false
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `numeric`  
**Aliases:** `opposite`, `numeric-is-opposite`
{: .member-reference }
