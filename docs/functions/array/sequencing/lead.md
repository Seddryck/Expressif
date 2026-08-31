---
layout: docs
title: "lead"
parent: "Sequencing functions"
grand_parent: "Array functions"
nav_order: 30
has_toc: false
permalink: /functions/array/sequencing/lead/
tags:
  - functions
  - array/sequencing
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
**Scope:** `array/sequencing`  
**Aliases:** `array-to-lead`
{: .member-reference }
