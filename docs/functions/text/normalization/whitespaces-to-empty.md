---
layout: docs
title: "whitespaces-to-empty"
parent: "Normalization functions"
grand_parent: "Text functions"
nav_order: 70
has_toc: false
permalink: /functions/text/normalization/whitespaces-to-empty/
tags:
  - functions
  - text/normalization
generated: true
---

```
text →
whitespaces-to-empty() → text
```

Returns the argument value except if this value only contains white-space characters then it returns `empty`.

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
"Hello World" | whitespaces-to-empty → "Hello World"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/normalization`  
**Aliases:** `blank-to-empty`
{: .member-reference }
