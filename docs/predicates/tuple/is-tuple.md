---
layout: docs
title: "is-tuple"
parent: "Tuple predicates"
grand_parent: "Predicates library"
nav_order: 20
has_toc: false
permalink: /predicates/tuple/is-tuple/
tags:
  - predicates
  - tuple
generated: true
---

```
any →
is-tuple() → boolean
```

Returns whether the input value is a tuple.

## Parameters



This predicate has no parameters.






## Examples

{% raw %}
```expressif
T(1, "foo") | is-tuple → #true
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `tuple`  
**Aliases:** None
{: .member-reference }
