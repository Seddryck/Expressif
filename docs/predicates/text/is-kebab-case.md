---
layout: docs
title: "is-kebab-case"
parent: "Text predicates"
grand_parent: "Predicates library"
nav_order: 80
has_toc: false
permalink: /predicates/text/is-kebab-case/
tags:
  - predicates
  - text
generated: true
---

```
text →
is-kebab-case() → boolean
```

Returns `true` when the complete input is a kebab-case identifier beginning with a letter. Words contain lowercase or uncased Unicode letters, combining marks, and numbers, separated by single hyphens. Returns `false` for null, empty, blank, punctuation, mixed separators, and leading, trailing, or repeated separators.



## Parameters



This predicate has no parameters.






## Examples

{% raw %}
```expressif
"first-name" | is-kebab-case → #true
"first--name" | is-kebab-case → #false
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `text`  
**Aliases:** None
{: .member-reference }
