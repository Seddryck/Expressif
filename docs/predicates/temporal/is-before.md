---
layout: docs
title: "is-before"
parent: "Temporal predicates"
grand_parent: "Predicates library"
nav_order: 30
has_toc: false
permalink: /predicates/temporal/is-before/
tags:
  - predicates
  - temporal
generated: true
---

```
is-before(
    reference: date-time
)
```

Returns true if the temporal value passed as argument is chronologically before the temporal value passed as parameter. Returns `false` otherwise.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `reference` | `date-time` | Yes | A temporal value to compare to the argument |



## Argument evaluation

- **`reference`:** Evaluated once in the enclosing context.



## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | is-before(#"2024-01-14 12:30:00") → #false
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `temporal`  
**Aliases:** `before`, `dateTime-is-before`
{: .member-reference }
