---
layout: docs
title: "prefix-new-line-if-missing"
parent: "Concatenation functions"
grand_parent: "Text functions"
nav_order: 60
has_toc: false
permalink: /functions/text/concatenation/prefix-new-line-if-missing/
tags:
  - functions
  - text/concatenation
generated: true
---

```
text →
prefix-new-line-if-missing() → text
```

Prefixes the argument with a CRLF sequence unless it already starts with CRLF. Preserves `null`.



## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
"Hello World" | prefix-new-line-if-missing → "\r\nHello World"
"\r\nHello World" | prefix-new-line-if-missing → "\r\nHello World"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/concatenation`  
**Aliases:** `text-to-prefix-new-line-if-missing`
{: .member-reference }
