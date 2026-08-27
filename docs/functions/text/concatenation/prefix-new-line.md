---
layout: docs
title: "prefix-new-line"
parent: "Concatenation functions"
grand_parent: "Text functions"
nav_order: 50
has_toc: false
permalink: /functions/text/concatenation/prefix-new-line/
tags:
  - functions
  - text/concatenation
generated: true
---

```
text →
prefix-new-line() → text
```

Returns the argument value preceeded by a space character. If the argument is `null`, it returns `null`.

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
"Hello World" | prefix-new-line → "Hello World" | prefix-new-line
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/concatenation`  
**Aliases:** `text-to-prefix-new-line`
{: .member-reference }
