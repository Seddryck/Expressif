---
layout: docs
title: "flat-map"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 40
has_toc: false
permalink: /functions/array/flat-map/
tags:
  - functions
  - array
generated: true
---

```
array →
flat-map(
    expression: expression
) → array
```

Evaluates an array-producing expression for each input element and concatenates the resulting arrays in order. Flattens one level, preserving nested arrays and null elements. Empty arrays contribute no elements. Throws an argument error when an expression result is not an array, including null or text.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `expression` | `expression` | Yes | Expression producing the array of output elements for each source element. |



## Argument evaluation

Visits each element of the array supplied as pipeline input to this flat-map call.

- **`expression`:** Evaluated once per visited element in source order, with that element as its pipeline input and argument context. Field references such as .orders read that element's fields.



## Examples

{% raw %}
```expressif
{{orders := {1, 2}}, {orders := {}}, {orders := {3}}} | flat-map(.orders) → {1, 2, 3}
{"one two", "three four"} | flat-map(tokenize(" ")) → {"one", "two", "three", "four"}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array`  
**Aliases:** None
{: .member-reference }
