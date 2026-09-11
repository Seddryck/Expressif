---
layout: docs
title: "is-pascal-case"
parent: "Text predicates"
grand_parent: "Predicates library"
nav_order: 100
has_toc: false
permalink: /predicates/text/is-pascal-case/
tags:
  - predicates
  - text
generated: true
---

```
text →
is-pascal-case() → boolean
```

Returns `true` when the complete input is a pascal-case identifier beginning with a letter. The initial letter is uppercase or titlecase; subsequent Unicode letters, combining marks, and numbers may include uppercase acronym runs, as in `HTTPServer`. Returns `false` for null, empty, blank, punctuation, mixed separators, and leading, trailing, or repeated separators.



## Parameters



This predicate has no parameters.






## Examples

{% raw %}
```expressif
"FirstName" | is-pascal-case → #true
"first--name" | is-pascal-case → #false
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `text`  
**Aliases:** None
{: .member-reference }
