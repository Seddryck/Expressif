---
layout: docs
title: "coerce-numeric"
parent: "Special functions"
grand_parent: "Functions library"
nav_order: 70
has_toc: false
permalink: /functions/special/coerce-numeric/
tags:
  - functions
  - special
generated: true
---

```
boolean | integer | numeric | text →
coerce-numeric() → numeric
```

Attempts to convert the input to a numeric value. Returns `null` when the input cannot be converted.

## Parameters



This function has no parameters.





## Examples

```expressif
"Hello World" | coerce-numeric → #null
```


**Kind:** Function  
**Scope:** `special`  
**Aliases:** `special-to-coerce-numeric`
{: .member-reference }
