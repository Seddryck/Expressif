---
layout: docs
title: "append-space"
parent: "Concatenation functions"
grand_parent: "Text functions"
nav_order: 30
has_toc: false
permalink: /functions/text/concatenation/append-space/
tags:
  - functions
  - text/concatenation
generated: true
---

```
text →
append-space() → text
```

Returns the argument value followed by a space character. If the argument is `null`, it returns the text specified as the parameter.

## Parameters



This function has no parameters.





## Examples

```expressif
"Hello World" | append-space → "Hello World "
```


**Kind:** Function  
**Scope:** `text/concatenation`  
**Aliases:** `text-to-append-space`
{: .member-reference }
