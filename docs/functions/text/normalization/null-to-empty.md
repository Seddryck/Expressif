---
layout: docs
title: "null-to-empty"
parent: "Normalization functions"
grand_parent: "Text functions"
nav_order: 40
has_toc: false
permalink: /functions/text/normalization/null-to-empty/
tags:
  - functions
  - text/normalization
generated: true
---

```
text →
null-to-empty() → text
```

Returns the argument value except if this value is `null` then it returns `empty`.

## Parameters



This function has no parameters.





## Examples

```expressif
"Hello World" | null-to-empty → "Hello World"
```


**Kind:** Function  
**Scope:** `text/normalization`  
**Aliases:** None
{: .member-reference }
