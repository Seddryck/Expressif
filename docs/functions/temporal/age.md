---
layout: docs
title: "age"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 10
has_toc: false
permalink: /functions/temporal/age/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
age() → integer
```

Returns the completed years between the argument dateTime and the current date. Returns `null` for null or future dates. In a non-leap year, a February 29 birthday is reached on February 28.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | age → 2
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `date-to-age`
{: .member-reference }
