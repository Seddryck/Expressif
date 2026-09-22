---
layout: docs
title: "matches-numeric"
parent: "Text predicates"
grand_parent: "Predicates library"
nav_order: 210
has_toc: false
permalink: /predicates/text/matches-numeric/
tags:
  - predicates
  - text
generated: true
---

```
matches-numeric()
```

Returns `true` if the text value passed as argument is a valid representation of a numeric in the culture specified as parameter. Returns `false` otherwise.



## Parameters



This predicate has no parameters.






## Examples

{% raw %}
```expressif
"Hello World" | matches-numeric → #false
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `text`  
**Aliases:** `text-matches-numeric`
{: .member-reference }
