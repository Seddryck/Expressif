---
layout: docs
title: "exclude-fields"
parent: "Record functions"
grand_parent: "Functions library"
nav_order: 20
has_toc: false
permalink: /functions/record/exclude-fields/
tags:
  - functions
  - record
generated: true
---

```
record →
exclude-fields(
    names: array
) → record
```

Returns all fields except those whose names appear in the supplied array, preserving input field order and ignoring unknown names.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `names` | `array` | Yes | Field names to remove. Unknown and duplicate names are ignored. |

## Argument evaluation

- **`names`:** Evaluated once in the enclosing context.

## Examples

{% raw %}
```expressif
{name := "Mons", temp := 18, pressure := 1012} | exclude-fields({"pressure"}) → {name := "Mons", temp := 18}
```
{% endraw %}


**Kind:** Function  
**Scope:** `record`  
**Aliases:** None
{: .member-reference }
