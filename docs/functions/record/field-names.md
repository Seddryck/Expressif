---
layout: docs
title: "field-names"
parent: "Record functions"
grand_parent: "Functions library"
nav_order: 40
has_toc: false
permalink: /functions/record/field-names/
tags:
  - functions
  - record
generated: true
---

```
record →
field-names() → array
```

Returns the names of all fields in the input record, preserving field order.



## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
{name := "Mons", temp := 18, pressure := 1012} | field-names → {"name", "temp", "pressure"}
```
{% endraw %}


**Kind:** Function  
**Scope:** `record`  
**Aliases:** None
{: .member-reference }
