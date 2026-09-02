---
layout: docs
title: "change-of-minute"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 60
has_toc: false
permalink: /functions/temporal/change-of-minute/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
change-of-minute() → date-time
```

returns a temporal value corresponding to the same instant of the argument value but with a new value for the second part.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:45" | change-of-minute(15) → #"2024-01-15 12:15:45"
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-change-of-minute`
{: .member-reference }
