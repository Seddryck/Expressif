---
layout: docs
title: "is-camel-case"
parent: "Text predicates"
grand_parent: "Predicates library"
nav_order: 40
has_toc: false
permalink: /predicates/text/is-camel-case/
tags:
  - predicates
  - text
generated: true
---

```
text →
is-camel-case() → boolean
```

Returns `true` when the complete input is a camel-case identifier beginning with a letter. The initial letter is lowercase; subsequent Unicode letters, combining marks, and numbers may include uppercase acronym runs, as in `httpServerURL`. Returns `false` for null, empty, blank, punctuation, mixed separators, and leading, trailing, or repeated separators.



## Parameters



This predicate has no parameters.






## Examples

{% raw %}
```expressif
"firstName" | is-camel-case → #true
"first--name" | is-camel-case → #false
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `text`  
**Aliases:** None
{: .member-reference }
