---
layout: docs
title: "prefix-space-if-missing"
parent: "Concatenation functions"
grand_parent: "Text functions"
nav_order: 80
has_toc: false
permalink: /functions/text/concatenation/prefix-space-if-missing/
tags:
  - functions
  - text/concatenation
generated: true
---

```
text →
prefix-space-if-missing() → text
```

Prefixes the argument with a space character unless it already starts with one. Preserves `null`.



## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
"Hello World" | prefix-space-if-missing → " Hello World"
" Hello World" | prefix-space-if-missing → " Hello World"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/concatenation`  
**Aliases:** `text-to-prefix-space-if-missing`
{: .member-reference }
