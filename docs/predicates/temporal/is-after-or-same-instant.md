---
layout: docs
title: "is-after-or-same-instant"
parent: "Temporal predicates"
grand_parent: "Predicates library"
nav_order: 20
has_toc: false
permalink: /predicates/temporal/is-after-or-same-instant/
tags:
  - predicates
  - temporal
generated: true
---

```
is-after-or-same-instant(
    reference: date-time
)
```

Returns true if the temporal value passed as argument is chronologically after the temporal value passed as parameter or if the two values represent the same instant . Returns `false` otherwise.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `reference` | `date-time` | Yes | A temporal value to compare to the argument. |



## Argument evaluation

- **`reference`:** Evaluated once in the enclosing context.



## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | is-after-or-same-instant(#"2024-01-14 12:30:00") → #true
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `temporal`  
**Aliases:** `after-or-same-instant`, `dateTime-is-after-or-same-instant`
{: .member-reference }
