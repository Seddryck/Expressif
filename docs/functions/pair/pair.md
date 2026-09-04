---
layout: docs
title: "pair"
parent: "Pair functions"
grand_parent: "Functions library"
nav_order: 10
has_toc: false
permalink: /functions/pair/pair/
tags:
  - functions
  - pair
generated: true
---

```
any →
pair(
    key: any,
    value: any
) → pair
```

Constructs a pair by evaluating a key expression and a value expression against the same input.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `key` | `any` | Yes | The expression whose evaluated result becomes the key. |
| `value` | `any` | Yes | The expression whose evaluated result becomes the value. |






## Examples

{% raw %}
```expressif
#null | pair("BE", 42) → ("BE" => 42)
{country := "BE", amount := 42} | pair(.country, .amount) → ("BE" => 42)
```
{% endraw %}


**Kind:** Function  
**Scope:** `pair`  
**Aliases:** None
{: .member-reference }
