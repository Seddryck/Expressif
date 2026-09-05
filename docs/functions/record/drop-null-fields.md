---
layout: docs
title: "drop-null-fields"
parent: "Record functions"
grand_parent: "Functions library"
nav_order: 10
has_toc: false
permalink: /functions/record/drop-null-fields/
tags:
  - functions
  - record
generated: true
---

```
record →
drop-null-fields() → record
```

Removes null-valued fields from the input record without traversing nested records or collections.



## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
{name := "Nikola", age := #null, active := #false} | drop-null-fields → {name := "Nikola", active := #false}
```
{% endraw %}


**Kind:** Function  
**Scope:** `record`  
**Aliases:** None
{: .member-reference }
