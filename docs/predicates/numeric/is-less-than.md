---
layout: docs
title: "is-less-than"
parent: "Numeric predicates"
grand_parent: "Predicates library"
nav_order: 40
has_toc: false
permalink: /predicates/numeric/is-less-than/
tags:
  - predicates
  - numeric
generated: true
---

```
is-less-than(
    reference: numeric
)
```

Returns true if the numeric value passed as argument is less than the numeric value passed as parameter. Returns `false` otherwise.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `reference` | `numeric` | Yes | A numeric value to compare to the argument. |






## Examples

{% raw %}
```expressif
10 | is-less-than(5) → #false
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `numeric`  
**Aliases:** `less-than`, `numeric-is-less-than`
{: .member-reference }
