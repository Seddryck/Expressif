---
layout: docs
title: "is-less-than-or-equal"
parent: "Numeric predicates"
grand_parent: "Predicates library"
nav_order: 50
has_toc: false
permalink: /predicates/numeric/is-less-than-or-equal/
tags:
  - predicates
  - numeric
generated: true
---

```
is-less-than-or-equal(
    reference: numeric
)
```

Returns true if the numeric value passed as argument is less than or equal to the numeric value passed as parameter. Returns `false` otherwise.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `reference` | `numeric` | Yes | A numeric value to compare to the argument. |






## Examples

{% raw %}
```expressif
10 | is-less-than-or-equal(5) → #false
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `numeric`  
**Aliases:** `less-than-or-equal`, `numeric-is-less-than-or-equal`
{: .member-reference }
