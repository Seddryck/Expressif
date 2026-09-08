---
layout: docs
title: "put-present"
parent: "Record functions"
grand_parent: "Functions library"
nav_order: 100
has_toc: false
permalink: /functions/record/put-present/
tags:
  - functions
  - record
generated: true
---

```
record →
put-present(
    ...assignments: entry
) → record
```

Assigns statically named fields only when they are present, including fields whose value is null.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `assignments` | `entry` | Variadic (one or more) | One or more named assignments applied only to fields already present. |

## Argument evaluation

- **`assignments`:** Each assignment uses the record entering this call and is evaluated only if its target field is present.

## Examples

{% raw %}
```expressif
{name := "Alice", age := #null} | put-present(age := 42) → {name := "Alice", age := 42}
```
{% endraw %}


**Kind:** Function  
**Scope:** `record`  
**Aliases:** None
{: .member-reference }
