---
layout: docs
title: "select-fields"
parent: "Record functions"
grand_parent: "Functions library"
nav_order: 130
has_toc: false
permalink: /functions/record/select-fields/
tags:
  - functions
  - record
generated: true
---

```
record →
select-fields(
    names: array
) → record
```

Returns only fields whose names appear in the supplied array, preserving input field order and ignoring unknown names.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `names` | `array` | Yes | Field names to retain. Unknown and duplicate names are ignored. |






## Examples

{% raw %}
```expressif
{name := "Mons", temp := 18, pressure := 1012} | select-fields({"temp", "pressure"}) → {temp := 18, pressure := 1012}
```
{% endraw %}


**Kind:** Function  
**Scope:** `record`  
**Aliases:** None
{: .member-reference }
