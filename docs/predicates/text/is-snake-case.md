---
layout: docs
title: "is-snake-case"
parent: "Text predicates"
grand_parent: "Predicates library"
nav_order: 110
has_toc: false
permalink: /predicates/text/is-snake-case/
tags:
  - predicates
  - text
generated: true
---

```
text →
is-snake-case() → boolean
```

Returns `true` when the complete input is a snake-case identifier beginning with a letter. Words contain lowercase or uncased Unicode letters, combining marks, and numbers, separated by single underscores. Returns `false` for null, empty, blank, punctuation, mixed separators, and leading, trailing, or repeated separators.



## Parameters



This predicate has no parameters.






## Examples

{% raw %}
```expressif
"first_name" | is-snake-case → #true
"first--name" | is-snake-case → #false
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `text`  
**Aliases:** None
{: .member-reference }
