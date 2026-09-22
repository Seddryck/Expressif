---
layout: docs
title: "is-yesterday"
parent: "Temporal predicates"
grand_parent: "Predicates library"
nav_order: 330
has_toc: false
permalink: /predicates/temporal/is-yesterday/
tags:
  - predicates
  - temporal
generated: true
---

```
is-yesterday()
```

Returns true if the date passed as argument is representing the previous date compared to the current date. Returns false otherwise.



## Parameters



This predicate has no parameters.






## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | is-yesterday → #false
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `temporal`  
**Aliases:** `yesterday`, `dateTime-is-yesterday`
{: .member-reference }
