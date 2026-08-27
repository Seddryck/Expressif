---
layout: docs
title: "html-to-text"
parent: "Encoding functions"
grand_parent: "Text functions"
nav_order: 10
has_toc: false
permalink: /functions/text/encoding/html-to-text/
tags:
  - functions
  - text/encoding
generated: true
---

```
text →
html-to-text() → text
```

Returns the argument value that has previously been HTML-encoded into a decoded string.

## Parameters



This function has no parameters.





## Examples

```expressif
"Hello World" | html-to-text → "Hello World"
```


**Kind:** Function  
**Scope:** `text/encoding`  
**Aliases:** None
{: .member-reference }
