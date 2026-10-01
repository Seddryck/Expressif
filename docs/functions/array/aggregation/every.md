---
layout: docs
title: "every"
parent: "Aggregation functions"
grand_parent: "Array functions"
nav_order: 80
has_toc: false
permalink: /functions/array/aggregation/every/
tags:
  - functions
  - array/aggregation
generated: true
---

```
boolean →
every() → boolean
```

Returns `true` only when every accumulated boolean value is `true`.



## Parameters



This function has no parameters.



## Examples

{% raw %}
```expressif
{#true, #true, #false} | fold(every) → #false
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
