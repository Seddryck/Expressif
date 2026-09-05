---
layout: docs
title: "put-absent"
parent: "Record functions"
grand_parent: "Functions library"
nav_order: 40
has_toc: false
permalink: /functions/record/put-absent/
tags:
  - functions
  - record
generated: true
---

```
record →
put-absent(
    ...assignments: entry
) → record
```

Assigns statically named fields only when they are absent; a present field containing null remains unchanged.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `assignments` | `entry` | Variadic (one or more) | One or more named assignments applied only to fields that are absent. |






## Examples

{% raw %}
```expressif
{name := "Alice"} | put-absent(age := 42) → {name := "Alice", age := 42}
```
{% endraw %}


**Kind:** Function  
**Scope:** `record`  
**Aliases:** None
{: .member-reference }
