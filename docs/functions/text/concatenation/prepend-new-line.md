---
layout: docs
title: "prepend-new-line"
parent: "Concatenation functions"
grand_parent: "Text functions"
nav_order: 80
has_toc: false
permalink: /functions/text/concatenation/prepend-new-line/
tags:
  - functions
  - text/concatenation
generated: true
---

```
text →
prepend-new-line() → text
```

Returns the argument value preceeded by a space character. If the argument is `null`, it returns the text specified as the parameter.

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
"Hello World" | prepend-new-line → "Hello World" | prepend-new-line
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/concatenation`  
**Aliases:** `text-to-prepend-new-line`
{: .member-reference }
