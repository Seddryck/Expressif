---
layout: docs
title: "prefix-space"
parent: "Concatenation functions"
grand_parent: "Text functions"
nav_order: 60
has_toc: false
permalink: /functions/text/concatenation/prefix-space/
tags:
  - functions
  - text/concatenation
generated: true
---

```
text →
prefix-space() → text
```

Returns the argument value preceeded by a space character. If the argument is `null`, it returns `null`.

## Parameters



This function has no parameters.





## Examples

{% raw %}
```expressif
"Hello World" | prefix-space → " Hello World"
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/concatenation`  
**Aliases:** `text-to-prefix-space`
{: .member-reference }
