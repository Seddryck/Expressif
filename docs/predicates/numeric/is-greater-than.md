---
layout: docs
title: "is-greater-than"
parent: "Numeric predicates"
grand_parent: "Predicates library"
nav_order: 20
has_toc: false
permalink: /predicates/numeric/is-greater-than/
tags:
  - predicates
  - numeric
generated: true
---

```
is-greater-than(
    reference: numeric
)
```

Returns true if the numeric value passed as argument is greater than the numeric value passed as parameter. Returns `false` otherwise.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `reference` | `numeric` | Yes | A numeric value to compare to the argument. |






## Examples

{% raw %}
```expressif
10 | is-greater-than(5) → #true
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `numeric`  
**Aliases:** `greater-than`, `numeric-is-greater-than`
{: .member-reference }
