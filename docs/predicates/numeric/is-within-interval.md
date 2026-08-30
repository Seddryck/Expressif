---
layout: docs
title: "is-within-interval"
parent: "Numeric predicates"
grand_parent: "Predicates library"
nav_order: 130
has_toc: false
permalink: /predicates/numeric/is-within-interval/
tags:
  - predicates
  - numeric
generated: true
---

```
is-within-interval(
    interval: any
)
```

Returns true if the numeric value passed as argument is between the lower bound and the upper bound defined in the interval. Returns `false` otherwise.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `interval` | `any` | Yes | A numeric interval to compare to the argument. |






## Examples

{% raw %}
```expressif
#null | is-within-interval(#null) → #false
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `numeric`  
**Aliases:** `within-interval`, `numeric-is-within-interval`
{: .member-reference }
