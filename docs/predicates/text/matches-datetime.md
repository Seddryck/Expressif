---
layout: docs
title: "matches-datetime"
parent: "Text predicates"
grand_parent: "Predicates library"
nav_order: 200
has_toc: false
permalink: /predicates/text/matches-datetime/
tags:
  - predicates
  - text
generated: true
---

```
matches-datetime()
```

Returns `true` if the text value passed as argument is a valid representation of a dateTime in the culture specified as parameter. The expected format is the concatenation of the ShortDatePattern, a space and the LongTimePattern. If the value is of type `DateTime`, it returns `true`. Returns `false` otherwise.



## Parameters



This predicate has no parameters.






## Examples

{% raw %}
```expressif
"Hello World" | matches-datetime → #false
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `text`  
**Aliases:** `text-matches-datetime`
{: .member-reference }
