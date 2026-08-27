---
layout: docs
title: "record"
parent: "Record functions"
grand_parent: "Functions library"
nav_order: 20
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
    entries: array
) → record
```

Creates a record by evaluating its named and spread entries against the input value. Later entries overwrite fields with the same name created by earlier entries.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `entries` | `array` | Yes | Factory that creates the named and spread entries used to build the record. |





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
