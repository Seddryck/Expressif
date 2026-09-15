---
layout: docs
title: "nest"
parent: "Dictionary functions"
grand_parent: "Functions library"
nav_order: 20
has_toc: false
permalink: /functions/dictionary/nest/
tags:
  - functions
  - dictionary
generated: true
---

```
dictionary →
nest() → dictionary
```

Restructures a dictionary with tuple keys into nested dictionaries, one level per tuple position, preserving key types and source insertion order.



## Parameters



This function has no parameters.



## Argument evaluation

Visits each entry of the dictionary supplied as pipeline input to this nest call in insertion order, using its tuple key positions to form nested dictionary levels.



## Behavior

An empty dictionary remains empty. Every key in a nonempty dictionary must be a tuple of the same arity, at least two; other keys cause an evaluation error. Each tuple's last position maps to its original value. Keys at every level use dictionary structural equality.



## Examples

{% raw %}
```expressif
!{(T("BE", 2025) => 100), (T("BE", 2026) => 120), (T("FR", 2025) => 80)} | nest → !{("BE" => !{(2025 => 100), (2026 => 120)}), ("FR" => !{(2025 => 80)})}
```
{% endraw %}


**Kind:** Function  
**Scope:** `dictionary`  
**Aliases:** None
{: .member-reference }
