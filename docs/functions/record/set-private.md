---
layout: docs
title: "set-private"
parent: "Record functions"
grand_parent: "Functions library"
nav_order: 160
has_toc: false
permalink: /functions/record/set-private/
tags:
  - functions
  - record
generated: true
---

```
record →
set-private(
    names?: array
) → record
```

Returns a new record by renaming selected public fields by adding a leading underscore, preserving field order and values. Omitting names converts all applicable fields. Existing destination names cause an evaluation error. Unlike public, this function does not remove fields.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `names` | `array` | No | Field names without the private underscore prefix. Missing, inapplicable, and duplicate names are ignored. An empty array changes no fields; omission selects all applicable fields. |

## Argument evaluation

- **`names`:** Evaluated once in the enclosing context.

## Examples

{% raw %}
```expressif
{id := 42, name := "Ada"} | set-private({"id"}) → {_id := 42, name := "Ada"}
```
{% endraw %}


**Kind:** Function  
**Scope:** `record`  
**Aliases:** None
{: .member-reference }
