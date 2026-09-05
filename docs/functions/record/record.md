---
layout: docs
title: "record"
parent: "Record functions"
grand_parent: "Functions library"
nav_order: 90
has_toc: false
permalink: /functions/record/record/
tags:
  - functions
  - record
generated: true
---

```
any →
record(
    ...entries: entry
) → record
```

Creates a record by evaluating its named and spread entries against the input value. Later entries overwrite fields with the same name created by earlier entries.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `entries` | `entry` | Variadic (zero or more) | Zero or more named or spread entries used to construct the resulting record. Each entry is evaluated against the input value. |






## Examples

{% raw %}
```expressif
{name := "Ada", score := 10} | record(name := "Ada", score := 10) → {name := "Ada", score := 10}
```
{% endraw %}


**Kind:** Function  
**Scope:** `record`  
**Aliases:** None
{: .member-reference }
