---
layout: docs
title: "suffix-new-line"
parent: "Concatenation functions"
grand_parent: "Text functions"
nav_order: 120
has_toc: false
permalink: /functions/text/concatenation/suffix-new-line/
tags:
  - functions
  - text/concatenation
generated: true
---

```
text →
suffix-new-line() → text
```

Returns the argument value followed by a space character. If the argument is `null`, it returns `null`.

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
"Hello World" | suffix-new-line → "Hello World" | suffix-new-line
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/concatenation`  
**Aliases:** `text-to-suffix-new-line`
{: .member-reference }
