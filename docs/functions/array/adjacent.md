---
layout: docs
title: "adjacent"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 10
has_toc: false
permalink: /functions/array/adjacent/
tags:
  - functions
  - array
generated: true
---

```
array →
adjacent(
    operation: expression
) → array
```

Evaluates an operation against every consecutive pair of input values. Returns `null` when the input cannot be evaluated.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `operation` | `expression` | Yes | Specifies the callable or open expression evaluated against each consecutive pair. |





## Examples

```expressif
{1, 2, 3} | adjacent(subtract) → {1, 1}
```


**Kind:** Function  
**Scope:** `array`  
**Aliases:** `adjacent`
{: .member-reference }
