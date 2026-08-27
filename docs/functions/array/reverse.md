---
layout: docs
title: "reverse"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 140
has_toc: false
permalink: /functions/array/reverse/
tags:
  - functions
  - array
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
**Scope:** `array`  
**Aliases:** `reverse`
{: .member-reference }
