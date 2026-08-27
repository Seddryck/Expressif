---
layout: docs
title: "coerce-text"
parent: "Special functions"
grand_parent: "Functions library"
nav_order: 80
has_toc: false
permalink: /functions/special/coerce-text/
tags:
  - functions
  - special
generated: true
---

```
boolean | date | date-time | integer | numeric | text | year-month →
coerce-text() → text
```

Attempts to convert the input to a text value. Returns `null` when the input cannot be converted.

## Parameters



This function has no parameters.





## Examples

```expressif
"Hello World" | coerce-text → "Hello World"
```


**Kind:** Function  
**Scope:** `special`  
**Aliases:** `special-to-coerce-text`
{: .member-reference }
