---
layout: docs
title: "with-position"
parent: "Sequencing functions"
grand_parent: "Array functions"
nav_order: 70
has_toc: false
permalink: /functions/array/sequencing/with-position/
tags:
  - functions
  - array/sequencing
generated: true
---

```
array →
with-position() → array
```

Returns each input item paired with its zero-based position as a tuple in `(position, value)` order. Preserves input order and cardinality. Position terminology distinguishes sequence locations from indexes used to accelerate searches. Returns `null` when the input cannot be evaluated.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
{"a", "b", "c"} | with-position | value-at(2) | tuple-first → 2
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/sequencing`  
**Aliases:** `with-position`
{: .member-reference }
