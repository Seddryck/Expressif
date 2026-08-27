---
layout: docs
title: "coerce-boolean"
parent: "Special functions"
grand_parent: "Functions library"
nav_order: 30
has_toc: false
permalink: /functions/special/coerce-boolean/
tags:
  - functions
  - special
generated: true
---

```
boolean | integer | numeric | text →
coerce-boolean() → boolean
```

Attempts to convert the input to a boolean value. Returns `null` when the input cannot be converted.

## Parameters



This function has no parameters.





## Examples

```expressif
"Hello World" | coerce-boolean → #null
```


**Kind:** Function  
**Scope:** `special`  
**Aliases:** `special-to-coerce-boolean`
{: .member-reference }
