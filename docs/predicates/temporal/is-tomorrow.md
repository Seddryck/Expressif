---
layout: docs
title: "is-tomorrow"
parent: "Temporal predicates"
grand_parent: "Predicates library"
nav_order: 190
has_toc: false
permalink: /predicates/temporal/is-tomorrow/
tags:
  - predicates
  - temporal
generated: true
---

```
is-tomorrow()
```

Returns true if the date passed as argument is representing the next date compared to the current date. Returns false otherwise.



## Parameters



This predicate has no parameters.






## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | is-tomorrow → #false
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `temporal`  
**Aliases:** `tomorrow`, `dateTime-is-tomorrow`
{: .member-reference }
