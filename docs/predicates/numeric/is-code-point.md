---
layout: docs
title: "is-code-point"
parent: "Numeric predicates"
grand_parent: "Predicates library"
nav_order: 5
has_toc: false
permalink: /predicates/numeric/is-code-point/
tags:
  - predicates
  - numeric
generated: true
---

```
numeric →
is-code-point() → boolean
```

Returns `true` when the input is an integer Unicode scalar value. Returns `false` otherwise.

## Parameters



This predicate has no parameters.






## Examples

{% raw %}
```expressif
128512 | is-code-point → #true
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `numeric`  
**Aliases:** `code-point`
{: .member-reference }
