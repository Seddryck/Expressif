---
layout: docs
title: "pairwise"
parent: "Sequencing functions"
grand_parent: "Array functions"
nav_order: 40
has_toc: false
permalink: /functions/array/sequencing/pairwise/
tags:
  - functions
  - array/sequencing
generated: true
---

```
array →
pairwise() → array
```

Returns each consecutive pair of input values as a tuple. Returns `null` when the input cannot be evaluated.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
{1, 2, 3} | pairwise → {T(1, 2), T(2, 3)}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/sequencing`  
**Aliases:** `pairwise`
{: .member-reference }
