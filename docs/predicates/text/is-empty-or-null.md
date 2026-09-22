---
layout: docs
title: "is-empty-or-null"
parent: "Text predicates"
grand_parent: "Predicates library"
nav_order: 70
has_toc: false
permalink: /predicates/text/is-empty-or-null/
tags:
  - predicates
  - text
generated: true
---

```
is-empty-or-null()
```

Returns `true` if argument value has a length of `0` or is `null`. Return `false` otherwise.



## Parameters



This predicate has no parameters.






## Examples

{% raw %}
```expressif
"Hello World" | is-empty-or-null → #false
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `text`  
**Aliases:** `empty-or-null`, `text-is-empty-or-null`
{: .member-reference }
