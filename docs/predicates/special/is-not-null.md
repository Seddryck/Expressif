---
layout: docs
title: "is-not-null"
parent: "Special predicates"
grand_parent: "Predicates library"
nav_order: 10
has_toc: false
permalink: /predicates/special/is-not-null/
tags:
  - predicates
  - special
generated: true
---

```
any →
is-not-null() → boolean
```

Returns true when the input does not satisfy is-null.



## Parameters



This predicate has no parameters.






## Examples

{% raw %}
```expressif
#null | is-not-null → #false
10 | is-not-null → #true
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `special`  
**Aliases:** None
{: .member-reference }
