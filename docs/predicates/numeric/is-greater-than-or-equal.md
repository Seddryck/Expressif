---
layout: docs
title: "is-greater-than-or-equal"
parent: "Numeric predicates"
grand_parent: "Predicates library"
nav_order: 30
has_toc: false
permalink: /predicates/numeric/is-greater-than-or-equal/
tags:
  - predicates
  - numeric
generated: true
---

```
is-greater-than-or-equal(
    reference: numeric
)
```

Returns true if the numeric value passed as argument is greater than or equal to the numeric value passed as parameter. Returns `false` otherwise.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `reference` | `numeric` | Yes | A numeric value to compare to the argument. |






## Examples

{% raw %}
```expressif
10 | is-greater-than-or-equal(5) → #true
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `numeric`  
**Aliases:** `greater-than-or-equal`, `numeric-is-greater-than-or-equal`
{: .member-reference }
