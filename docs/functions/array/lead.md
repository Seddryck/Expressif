---
layout: docs
title: "lead"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 110
has_toc: false
permalink: /functions/array/lead/
tags:
  - functions
  - array
generated: true
---

```
array →
lead() → array
```

Returns the next value for each input element. The last output value is `null` because there is no next element. Preserves input cardinality (one output item per input item). Returns `null` when the input is not an enumerable or is a string.

## Parameters



This function has no parameters.






## Examples

{% raw %}
```expressif
{1, 2, 3} | lead → {2, 3, #null}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array`  
**Aliases:** `array-to-lead`
{: .member-reference }
