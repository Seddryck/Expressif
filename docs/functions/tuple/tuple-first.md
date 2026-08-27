---
layout: docs
title: "tuple-first"
parent: "Tuple functions"
grand_parent: "Functions library"
nav_order: 70
has_toc: false
permalink: /functions/tuple/tuple-first/
tags:
  - functions
  - tuple
generated: true
---

```
tuple →
tuple-first() → any
```

Returns the first field of a tuple. Returns `null` when the input is not a tuple.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
T(10, 20, 30) | tuple-first → 10
```
{% endraw %}


**Kind:** Function  
**Scope:** `tuple`  
**Aliases:** `tuple-first`
{: .member-reference }
