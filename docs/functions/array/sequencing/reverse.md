---
layout: docs
title: "reverse"
parent: "Sequencing functions"
grand_parent: "Array functions"
nav_order: 60
has_toc: false
permalink: /functions/array/sequencing/reverse/
tags:
  - functions
  - array/sequencing
generated: true
---

```
array →
reverse() → array
```

Returns the input enumerable with elements emitted in the opposite order. Preserves input cardinality (one output item per input item). Returns `null` when the input is not an enumerable or is a string.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
{1, 2, 3} | reverse → {3, 2, 1}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/sequencing`  
**Aliases:** `reverse`
{: .member-reference }
