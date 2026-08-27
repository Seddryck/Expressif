---
layout: docs
title: "distinct"
parent: "Set functions"
grand_parent: "Array functions"
nav_order: 30
has_toc: false
permalink: /functions/array/set/distinct/
tags:
  - functions
  - array/set
generated: true
---

```
array →
distinct() → array
```

Returns the unique values from the input array in the order of their first occurrence. Returns `null` when the input cannot be evaluated.

## Parameters



This function has no parameters.





## Examples

```expressif
{1, 2, 3} | distinct → {1, 2, 3}
```


**Kind:** Function  
**Scope:** `array/set`  
**Aliases:** `distinct`
{: .member-reference }
