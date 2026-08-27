---
layout: docs
title: "retain-numeric"
parent: "Filtering functions"
grand_parent: "Text functions"
nav_order: 40
has_toc: false
permalink: /functions/text/filtering/retain-numeric/
tags:
  - functions
  - text/filtering
generated: true
---

```
text →
retain-numeric() → text
```

Returns the input string with all non-numeric characters removed, leaving only digits (0-9).. If the argument is `null`, it returns `null`.

## Parameters



This function has no parameters.





## Examples

```expressif
"Hello World" | retain-numeric → "(empty)"
```


**Kind:** Function  
**Scope:** `text/filtering`  
**Aliases:** `text-to-retain-numeric`
{: .member-reference }
