---
layout: docs
title: "month"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 300
has_toc: false
permalink: /functions/temporal/month/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
month() → text
```

returns a textual value at format MM representing the month of the date passed as the argument

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | month → "01"
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** `dateTime-to-month`
{: .member-reference }
