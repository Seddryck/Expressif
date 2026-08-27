---
layout: docs
title: "tuple-second"
parent: "Tuple functions"
grand_parent: "Functions library"
nav_order: 80
has_toc: false
permalink: /functions/tuple/tuple-second/
tags:
  - functions
  - tuple
generated: true
---

```
tuple →
tuple-second() → any
```

Returns the second field of a tuple. Returns `null` when the input is not a tuple.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
T(10, 20, 30) | tuple-second → 20
```
{% endraw %}


**Kind:** Function  
**Scope:** `tuple`  
**Aliases:** `tuple-second`
{: .member-reference }
