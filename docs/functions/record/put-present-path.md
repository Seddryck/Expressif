---
layout: docs
title: "put-present-path"
parent: "Record functions"
grand_parent: "Functions library"
nav_order: 80
has_toc: false
permalink: /functions/record/put-present-path/
tags:
  - functions
  - record
generated: true
---

```
record →
put-present-path(
    path: expression,
    value: expression
) → record
```

Assigns the field at a dynamic path only when the final segment is present, including when its value is null.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `path` | `expression` | Yes | An expression producing non-empty text for one literal segment or a non-empty tuple of non-empty text segments. |
| `value` | `expression` | Yes | The expression producing the assigned value from the original input record. |






## Examples

{% raw %}
```expressif
{age := #null} | put-present-path("age", 42) → {age := 42}
```
{% endraw %}


**Kind:** Function  
**Scope:** `record`  
**Aliases:** None
{: .member-reference }
