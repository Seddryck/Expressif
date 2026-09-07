---
layout: docs
title: "nested-field"
parent: "Record functions"
grand_parent: "Functions library"
nav_order: 50
has_toc: false
permalink: /functions/record/nested-field/
tags:
  - functions
  - record
generated: true
---

```
any →
nested-field(
    ...path: text
) → any
```

Returns the value at a nested field path in the input record or object, or null when the path cannot be resolved.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `path` | `text` | Variadic (one or more) | One or more literal field names in traversal order. Spread arguments expand arrays of names in place. |





## Behavior

Path expressions are evaluated from left to right against the original input before traversal. Explicit and spread arguments may be mixed; spread uses the shared array expansion rules. Each text segment is one literal field name, including dots and empty text. The result preserves the selected value and its runtime type, including structured values and null. Field-name matching and unresolved paths follow field semantics. An empty expanded path or a non-text segment raises an argument error; unsupported spread values raise a spread error. Only positional arguments are accepted.



## Examples

{% raw %}
```expressif
{customer := {address := {city := "Brussels"}}} | nested-field("customer", "address", "city") → "Brussels"
{customer := {address := {city := "Brussels"}}} | nested-field(...{"customer", "address", "city"}) → "Brussels"
{customer := {address := {city := "Brussels"}}} | nested-field("customer", ...{"address", "city"}) → "Brussels"
```
{% endraw %}


**Kind:** Function  
**Scope:** `record`  
**Aliases:** None
{: .member-reference }
