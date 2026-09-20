---
layout: docs
title: "is-empty"
parent: "Text predicates"
grand_parent: "Predicates library"
nav_order: 60
has_toc: false
permalink: /predicates/text/is-empty/
tags:
  - predicates
  - text
generated: true
---

```
is-empty()
```

Returns `true` if argument value has a length of `0`. Return `false` otherwise.



## Parameters



This predicate has no parameters.






## Examples

{% raw %}
```expressif
"Hello World" | is-empty → #false
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `text`  
**Aliases:** `empty`, `text-is-empty`
{: .member-reference }
