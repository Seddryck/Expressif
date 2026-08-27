---
layout: docs
title: "coerce-time"
parent: "Special functions"
grand_parent: "Functions library"
nav_order: 90
has_toc: false
permalink: /functions/special/coerce-time/
tags:
  - functions
  - special
generated: true
---

```
date-time | text | time →
coerce-time() → time
```

Attempts to convert the input to a time value. Returns `null` when the input cannot be converted.

## Parameters



This function has no parameters.





## Examples

```expressif
"Hello World" | coerce-time → #null
```


**Kind:** Function  
**Scope:** `special`  
**Aliases:** `special-to-coerce-time`
{: .member-reference }
