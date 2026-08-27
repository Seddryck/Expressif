---
layout: docs
title: "set-to-local"
parent: "Temporal functions"
grand_parent: "Functions library"
nav_order: 490
has_toc: false
permalink: /functions/temporal/set-to-local/
tags:
  - functions
  - temporal
generated: true
---

```
date-time →
set-to-local() → date-time
```

Returns the dateTime passed as argument without changing the current hours/minutes and sets the kind to local

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
#"2024-01-15 12:30:00" | set-to-local → #"2024-01-15 12:30:00"
```
{% endraw %}


**Kind:** Function  
**Scope:** `temporal`  
**Aliases:** None
{: .member-reference }
