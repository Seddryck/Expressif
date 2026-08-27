---
layout: docs
title: "coerce-datetime"
parent: "Special functions"
grand_parent: "Functions library"
nav_order: 50
has_toc: false
permalink: /functions/special/coerce-datetime/
tags:
  - functions
  - special
generated: true
---

```
date | date-time | text | year-month →
coerce-datetime() → date-time
```

Attempts to convert the input to a date-time value. Returns `null` when the input cannot be converted.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
"Hello World" | coerce-datetime → #null
```
{% endraw %}


**Kind:** Function  
**Scope:** `special`  
**Aliases:** `special-to-coerce-datetime`
{: .member-reference }
