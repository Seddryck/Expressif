---
layout: docs
title: "common-prefix"
parent: "Array accumulators"
grand_parent: "Accumulators library"
nav_order: 30
has_toc: false
permalink: /accumulators/array/common-prefix/
tags:
  - accumulators
  - array
generated: true
---

```
text →
common-prefix() → text
```

Returns the longest prefix shared by all accumulated strings.



## Parameters



This accumulator has no parameters.





## Behavior

Empty input returns null. A single string is returned unchanged. Nonempty input with no shared text, including an empty string, returns empty text. Comparison is ordinal and case-sensitive, without Unicode normalization. The result is independent of input order. Null and non-string elements throw InvalidCastException, even after the running result becomes empty.



## Examples

{% raw %}
```expressif
{"interact", "internet", "internal"} | fold(common-prefix) → "inter"
```
{% endraw %}


**Kind:** Accumulator  
**Scope:** `array`  
**Aliases:** `common-prefix`
{: .member-reference }
