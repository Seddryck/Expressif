---
layout: docs
title: "arity"
parent: "Tuple functions"
grand_parent: "Functions library"
nav_order: 20
has_toc: false
permalink: /functions/tuple/arity/
tags:
  - functions
  - tuple
generated: true
---

```
tuple →
arity() → integer
```

Returns the number of positional elements in the input tuple.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
T(10, "foo", #true) | arity → 3
```
{% endraw %}


**Kind:** Function  
**Scope:** `tuple`  
**Aliases:** `arity`
{: .member-reference }
