---
layout: docs
title: "split-lengths"
parent: "Partitioning functions"
grand_parent: "Text functions"
nav_order: 10
has_toc: false
permalink: /functions/text/partitioning/split-lengths/
tags:
  - functions
  - text/partitioning
generated: true
---

```
text →
split-lengths(
    ...lengths: integer
) → array
```

Splits text into consecutive nonempty segments of the requested lengths, preserving any remaining text as a final segment. Returns an empty array for null or empty input and null for invalid lengths.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `lengths` | `integer` | Variadic (zero or more) | Zero or more strictly positive character counts, consumed in order. Spread arrays expand lengths in place. |





## Behavior

All lengths are validated before splitting, including lengths after the input would be exhausted. Invalid, nonintegral, zero, or negative lengths return `null`. A short final segment consumes the available text; an exact fit has no empty remainder. With no lengths, nonempty input returns one segment. Empty and null input return an empty array when lengths are valid. Blank input represents one space; literal whitespace is preserved. Character counts use UTF-16 code units, like `first-chars` and `skip-first-chars`. Spread arguments preserve declaration order; an array without spread is not an integer length. Unlike delimiter-removing tokenization, concatenating the segments reconstructs the original text.



## Examples

{% raw %}
```expressif
"abcdefghijkl" | split-lengths(5, 3) → {"abcde", "fgh", "ijkl"}
"abcdefgh" | split-lengths(5, 3) → {"abcde", "fgh"}
"abcdef" | split-lengths(5, 3) → {"abcde", "f"}
"abc" | split-lengths → {"abc"}
"" | split-lengths(5) → {}
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/partitioning`  
**Aliases:** None
{: .member-reference }
