---
layout: docs
title: "prepend-space"
parent: "Concatenation functions"
grand_parent: "Text functions"
nav_order: 90
has_toc: false
permalink: /functions/text/concatenation/prepend-space/
tags:
  - functions
  - text/concatenation
generated: true
---

```
text →
prepend-space() → text
```

Returns the argument value preceeded by a space character. If the argument is `null`, it returns the text specified as the parameter.

## Parameters



This function has no parameters.





## Examples

```expressif
"Hello World" | prepend-space → " Hello World"
```


**Kind:** Function  
**Scope:** `text/concatenation`  
**Aliases:** `text-to-prepend-space`
{: .member-reference }
