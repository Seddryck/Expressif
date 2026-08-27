---
layout: docs
title: "empty-to-null"
parent: "Normalization functions"
grand_parent: "Text functions"
nav_order: 30
has_toc: false
permalink: /functions/text/normalization/empty-to-null/
tags:
  - functions
  - text/normalization
generated: true
---

```
text →
empty-to-null() → text
```

Returns the argument value except if this value is `empty` then it returns `null`.

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
"Hello World" | empty-to-null → "Hello World"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/normalization`  
**Aliases:** None
{: .member-reference }
