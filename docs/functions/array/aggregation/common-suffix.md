---
layout: docs
title: "common-suffix"
parent: "Aggregation functions"
grand_parent: "Array functions"
nav_order: 50
has_toc: false
permalink: /functions/array/aggregation/common-suffix/
tags:
  - functions
  - array/aggregation
generated: true
---

```
text →
common-suffix() → nullable<text>
```

Returns the longest suffix shared by all accumulated strings.



## Parameters



This function has no parameters.



## Examples

{% raw %}
```expressif
{"running", "walking", "talking"} | fold(common-suffix) → "ing"
```
{% endraw %}

## Value shape

- Pipeline input: `text`
- Returns: `nullable<text>`





## Aggregation support

This function supports incremental aggregation and can be used with `fold`, `scan`, and `broadcast`.




## Behavior

Empty input returns null. A single string is returned unchanged. Nonempty input with no shared text, including an empty string, returns empty text. Comparison is ordinal and case-sensitive, without Unicode normalization. The result is independent of input order. Null and non-string elements throw InvalidCastException, even after the running result becomes empty.

**Kind:** Function  
**Scope:** `array/aggregation`  
**Aliases:** None
{: .member-reference }
