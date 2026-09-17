---
layout: docs
title: "pairs"
parent: "Record functions"
grand_parent: "Functions library"
nav_order: 70
has_toc: false
permalink: /functions/record/pairs/
tags:
  - functions
  - record
generated: true
---

```
record →
pairs() → array
```

Converts all record fields to pairs, preserving field names, values, and order.



## Parameters



This function has no parameters.



## Argument evaluation

Visits every field of the record supplied as pipeline input to this pairs call, in field order, including private fields.



## Behavior

Each pair has the original field name as its key and the unchanged field value as its value, including null. An empty record produces an empty array. Non-record input is rejected.



## Examples

{% raw %}
```expressif
{foo := 1, bar := 2} | pairs → {("foo" => 1), ("bar" => 2)}
{_id := 42, name := "Cedric"} | pairs | from-pairs → {_id := 42, name := "Cedric"}
```
{% endraw %}


**Kind:** Function  
**Scope:** `record`  
**Aliases:** None
{: .member-reference }
