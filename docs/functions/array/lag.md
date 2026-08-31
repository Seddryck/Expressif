---
layout: docs
title: "lag"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 100
has_toc: false
permalink: /functions/array/lag/
tags:
  - functions
  - array
generated: true
---

```
array →
lag() → array
```

Returns the previous value for each input element. The first output value is `null` because there is no previous element. Preserves input cardinality (one output item per input item). Returns `null` when the input is not an enumerable or is a string.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
{1, 2, 3} | lag → {#null, 1, 2}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array`  
**Aliases:** `array-to-lag`
{: .member-reference }
