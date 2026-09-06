---
layout: docs
title: "public"
parent: "Record functions"
grand_parent: "Functions library"
nav_order: 50
has_toc: false
permalink: /functions/record/public/
tags:
  - functions
  - record
generated: true
---

```
record →
public() → record
```

Returns a new record without fields whose names start with an underscore, preserving public field order and values. Unlike set-public and set-private, this function removes fields rather than renaming them.



## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
{foo := 10, _tmp := 20, bar := 30} | public → {foo := 10, bar := 30}
```
{% endraw %}


**Kind:** Function  
**Scope:** `record`  
**Aliases:** None
{: .member-reference }
