---
layout: docs
title: "broadcast"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 30
has_toc: false
permalink: /functions/array/broadcast/
tags:
  - functions
  - array
generated: true
---

```
array →
broadcast(
    accumulator: accumulator
) → array
```

Executes an accumulator once over the full input enumerable, then returns the final accumulated value repeated once for each input element. Returns `null` when the input is not an enumerable or is a string.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `accumulator` | `accumulator` | Yes | Factory that creates the accumulator instance used for the broadcast execution. |





**Kind:** Function  
**Scope:** `array`  
**Aliases:** `array-to-broadcast`
{: .member-reference }
