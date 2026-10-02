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
T →
pair(
    key: any,
    value: any
) → pair<K, V>
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

## Value shape

- Pipeline input: `T`
- Returns: `pair<K, V>`
- `key`: Receives `T` and returns `K`.
- `value`: Receives `T` and returns `V`.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.




## Argument evaluation

- **`key`:** Evaluated once against the value entering this call.
- **`value`:** Evaluated once against the value entering this call.

**Kind:** Function  
**Scope:** `pair`  
**Aliases:** None
{: .member-reference }
