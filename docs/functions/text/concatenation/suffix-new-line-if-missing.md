---
layout: docs
title: "suffix-new-line-if-missing"
parent: "Concatenation functions"
grand_parent: "Text functions"
nav_order: 150
has_toc: false
permalink: /functions/text/concatenation/suffix-new-line-if-missing/
tags:
  - functions
  - text/concatenation
generated: true
---

```
text →
suffix-new-line-if-missing() → text
```

Suffixes the argument with a CRLF sequence unless it already ends with CRLF. Preserves `null`.



## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
"Hello World" | suffix-new-line-if-missing → "Hello World" | suffix-new-line
"Hello World" | suffix-new-line | suffix-new-line-if-missing → "Hello World" | suffix-new-line
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/concatenation`  
**Aliases:** `text-to-suffix-new-line-if-missing`
{: .member-reference }
