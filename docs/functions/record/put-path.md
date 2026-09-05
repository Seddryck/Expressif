---
layout: docs
title: "put-path"
parent: "Record functions"
grand_parent: "Functions library"
nav_order: 60
has_toc: false
permalink: /functions/record/put-path/
tags:
  - functions
  - record
generated: true
---

```
record →
put-path(
    path: expression,
    value: expression
) → record
```

Creates or replaces the field at a dynamic path. Text is one literal segment; a tuple supplies nested segments, creating missing intermediate records.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `path` | `expression` | Yes | An expression producing non-empty text for one literal segment or a non-empty tuple of non-empty text segments. |
| `value` | `expression` | Yes | The expression producing the assigned value from the original input record. |






## Examples

{% raw %}
```expressif
{customer := {address := {city := "Brussels"}}} | put-path(T("customer", "address", "city"), "Ghent") → {customer := {address := {city := "Ghent"}}}
```
{% endraw %}


**Kind:** Function  
**Scope:** `record`  
**Aliases:** None
{: .member-reference }
