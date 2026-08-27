---
layout: docs
title: "tuple-first"
parent: "Special functions"
grand_parent: "Functions library"
nav_order: 130
has_toc: false
permalink: /functions/special/tuple-first/
tags:
  - functions
  - special
generated: true
---

```
tuple →
tuple-first() → any
```

Returns the first field of a tuple. Returns `null` when the input is not a tuple.

## Parameters



This function has no parameters.





## Examples

```expressif
T(10, 20, 30) | tuple-first → 10
```


**Kind:** Function  
**Scope:** `special`  
**Aliases:** `tuple-first`
{: .member-reference }
