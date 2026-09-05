---
layout: docs
title: "not"
parent: "Boolean predicates"
grand_parent: "Predicates library"
nav_order: 100
has_toc: false
permalink: /predicates/boolean/not/
tags:
  - predicates
  - boolean
generated: true
---

```
not()
```

Returns the logical negation of the Boolean-converted input. Null-like and unconvertible values convert to `false`; zero converts to `false`; nonzero numbers and recognized true text convert to `true`.



## Parameters



This predicate has no parameters.






## Examples

{% raw %}
```expressif
#true | not → #false
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `boolean`  
**Aliases:** None
{: .member-reference }
