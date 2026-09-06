---
layout: docs
title: "common-suffix"
parent: "Array accumulators"
grand_parent: "Accumulators library"
nav_order: 40
has_toc: false
permalink: /accumulators/array/common-suffix/
tags:
  - accumulators
  - array
generated: true
---

```
text →
common-suffix() → text
```

Returns the longest suffix shared by all accumulated strings.



## Parameters



This accumulator has no parameters.





## Behavior

Empty input returns null. A single string is returned unchanged. Nonempty input with no shared text, including an empty string, returns empty text. Comparison is ordinal and case-sensitive, without Unicode normalization. The result is independent of input order. Null and non-string elements throw InvalidCastException, even after the running result becomes empty.



## Examples

{% raw %}
```expressif
{"running", "walking", "talking"} | fold(common-suffix) → "ing"
```
{% endraw %}


**Kind:** Accumulator  
**Scope:** `array`  
**Aliases:** `common-suffix`
{: .member-reference }
