---
layout: docs
title: "put-absent-path"
parent: "Record functions"
grand_parent: "Functions library"
nav_order: 50
has_toc: false
permalink: /functions/record/put-absent-path/
tags:
  - functions
  - record
generated: true
---

```
record →
put-absent-path(
    path: expression,
    value: expression
) → record
```

Assigns the field at a dynamic path only when the final segment is absent, creating missing intermediate records.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `path` | `expression` | Yes | An expression producing non-empty text for one literal segment or a non-empty tuple of non-empty text segments. |
| `value` | `expression` | Yes | The expression producing the assigned value from the original input record. |






## Examples

{% raw %}
```expressif
{name := "Alice"} | put-absent-path(T("details", "age"), 42) → {name := "Alice", details := {age := 42}}
```
{% endraw %}


**Kind:** Function  
**Scope:** `record`  
**Aliases:** None
{: .member-reference }
