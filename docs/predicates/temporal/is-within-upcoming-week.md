---
layout: docs
title: "is-within-upcoming-week"
parent: "Temporal predicates"
grand_parent: "Predicates library"
nav_order: 310
has_toc: false
permalink: /predicates/temporal/is-within-upcoming-week/
tags:
  - predicates
  - temporal
generated: true
---

```
is-within-upcoming-week()
```

Returns true if the date passed as argument is part of the week following the current week. A week is starting on Monday and ending on Sunday. Returns false otherwise.



## Parameters



This predicate has no parameters.






## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | is-within-upcoming-week → #false
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `temporal`  
**Aliases:** `within-upcoming-week`, `dateTime-is-within-upcoming-week`
{: .member-reference }
