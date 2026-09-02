---
layout: docs
title: "datetime-to-date"
parent: "Conversion functions"
grand_parent: "Temporal functions"
nav_order: 10
has_toc: false
permalink: /functions/temporal/conversion/datetime-to-date/
tags:
  - functions
  - temporal/conversion
generated: true
---

```
date-time →
datetime-to-date() → date
```

Returns the date at midnight of the argument dateTime.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | datetime-to-date → #"2024-01-15 00:00:00"
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal/conversion`  
**Aliases:** `dateTime-to-date`
{: .member-reference }
