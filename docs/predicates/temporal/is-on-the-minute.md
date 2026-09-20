---
layout: docs
title: "is-on-the-minute"
parent: "Temporal predicates"
grand_parent: "Predicates library"
nav_order: 160
has_toc: false
permalink: /predicates/temporal/is-on-the-minute/
tags:
  - predicates
  - temporal
generated: true
---

```
is-on-the-minute()
```

Returns `true` if the argument is of type `DateTime` and the seconds and milliseconds are all set at `0`. Returns `false` otherwise.



## Parameters



This predicate has no parameters.






## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | is-on-the-minute → #true
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `temporal`  
**Aliases:** `on-the-minute`, `dateTime-is-on-the-minute`
{: .member-reference }
