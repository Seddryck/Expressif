---
layout: docs
title: "put"
parent: "Record functions"
grand_parent: "Functions library"
nav_order: 30
has_toc: false
permalink: /functions/record/put/
tags:
  - functions
  - record
generated: true
---

```
record →
put(
    ...assignments: entry
) → record
```

Creates or replaces statically named fields while preserving every other field. Assignment expressions are evaluated against the original input record.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `assignments` | `entry` | Variadic (one or more) | One or more named assignments evaluated against the original input record. |






## Examples

{% raw %}
```expressif
{name := "Alice", age := 41} | put(age := .age | add(1)) → {name := "Alice", age := 42}
```
{% endraw %}


**Kind:** Function  
**Scope:** `record`  
**Aliases:** None
{: .member-reference }
