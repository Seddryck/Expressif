---
layout: docs
title: "common-prefix"
parent: "Aggregation functions"
grand_parent: "Array functions"
nav_order: 40
has_toc: false
permalink: /functions/array/aggregation/common-prefix/
tags:
  - functions
  - array/aggregation
generated: true
---

```
text →
common-prefix() → nullable<text>
```

Returns the longest prefix shared by all accumulated strings.



## Parameters



This function has no parameters.



## Value shape

- Pipeline input: `text`
- Returns: `nullable<text>`





## Aggregation support

This function supports incremental aggregation and can be used with `fold`, `scan`, and `broadcast`.




## Behavior

Empty input returns null. A single string is returned unchanged. Nonempty input with no shared text, including an empty string, returns empty text. Comparison is ordinal and case-sensitive, without Unicode normalization. The result is independent of input order. Null and non-string elements throw InvalidCastException, even after the running result becomes empty.



## Examples

{% raw %}
```expressif
{"interact", "internet", "internal"} | fold(common-prefix) → "inter"
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/aggregation`  
**Aliases:** None
{: .member-reference }
