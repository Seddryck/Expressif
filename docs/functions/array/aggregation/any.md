---
layout: docs
title: "any"
parent: "Aggregation functions"
grand_parent: "Array functions"
nav_order: 10
has_toc: false
permalink: /functions/array/aggregation/any/
tags:
  - functions
  - array/aggregation
generated: true
---

```
boolean →
any() → boolean
```

Returns `true` when at least one accumulated boolean value is `true`.



## Parameters



This function has no parameters.



## Examples

{% raw %}
```expressif
{#false, #true, #false} | fold(any) → #true
```
{% endraw %}

## Value shape

- Pipeline input: `boolean`
- Returns: `boolean`





## Aggregation support

This function supports incremental aggregation and can be used with `fold`, `scan`, and `broadcast`.

**Kind:** Function  
**Scope:** `array/aggregation`  
**Aliases:** None
{: .member-reference }
