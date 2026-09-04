---
layout: docs
title: "suffix-space-if-missing"
parent: "Concatenation functions"
grand_parent: "Text functions"
nav_order: 170
has_toc: false
permalink: /functions/text/concatenation/suffix-space-if-missing/
tags:
  - functions
  - text/concatenation
generated: true
---

```
text →
suffix-space-if-missing() → text
```

Suffixes the argument with a space character unless it already ends with one. Preserves `null`.



## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
"Hello World" | suffix-space-if-missing → "Hello World "
"Hello World " | suffix-space-if-missing → "Hello World "
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/concatenation`  
**Aliases:** `text-to-suffix-space-if-missing`
{: .member-reference }
