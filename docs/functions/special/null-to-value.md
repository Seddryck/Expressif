---
layout: docs
title: "null-to-value"
parent: "Special functions"
grand_parent: "Functions library"
nav_order: 110
has_toc: false
permalink: /functions/special/null-to-value/
tags:
  - functions
  - special
generated: true
---

```
any →
null-to-value() → text
```

Returns the value passed as argument, except if the value is `null` then it returns `value`.

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
#null | null-to-value → "(value)"
```
{% endraw %}


**Kind:** Function  
**Scope:** `special`  
**Aliases:** None
{: .member-reference }
