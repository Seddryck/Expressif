---
layout: docs
title: "change-of-hour"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 50
has_toc: false
permalink: /functions/temporal/change-of-hour/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
change-of-hour() → date-time
```

returns a temporal value corresponding to the same instant of the argument value but with a new value for the second part.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:45" | change-of-hour(8) → #"2024-01-15 08:30:45"
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-change-of-hour`
{: .member-reference }
