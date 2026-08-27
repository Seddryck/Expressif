---
layout: docs
title: "fold"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 70
has_toc: false
permalink: /functions/array/fold/
tags:
  - functions
  - array
generated: true
---

```
array →
fold(
    accumulator: accumulator
) → any
```

Executes an accumulator once over the full input enumerable and returns the final accumulated value. Returns `null` when the input is not an enumerable or is a string.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `accumulator` | `accumulator` | Yes | Factory that creates the accumulator instance used for the fold execution. |





**Kind:** Function  
**Scope:** `array`  
**Aliases:** `array-to-fold`
{: .member-reference }
