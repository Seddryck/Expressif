---
layout: docs
title: "field"
parent: "Record functions"
grand_parent: "Functions library"
nav_order: 10
has_toc: false
permalink: /functions/record/field/
tags:
  - functions
  - record
generated: true
---

```
any →
field(
    name: text
) → any
```

Returns the value of the named field from the input record or object. Returns `null` when the field does not exist or the input does not expose named values.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `name` | `text` | Yes | Name of the field to retrieve from the input. |





## Examples

{% raw %}
```expressif
{name := "Ada", score := 10} | field("name") → "Ada"
```
{% endraw %}


**Kind:** Function  
**Scope:** `record`  
**Aliases:** None
{: .member-reference }
