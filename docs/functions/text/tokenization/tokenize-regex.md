---
layout: docs
title: "tokenize-regex"
parent: "Tokenization functions"
grand_parent: "Text functions"
nav_order: 100
has_toc: false
permalink: /functions/text/tokenization/tokenize-regex/
tags:
  - functions
  - text/tokenization
generated: true
---

```
text →
tokenize-regex(
    pattern: text
) → array
```

Returns segments separated by regular expression matches in source order. Preserves spaces and empty segments without including captured delimiters. Returns an empty array for null or empty input.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `pattern` | `text` | Yes | The .NET regular expression identifying delimiters. Matching is case-sensitive unless inline options specify otherwise. |





## Behavior

Zero-width delimiters split at each match position with forward progress. Invalid patterns raise a regular expression error when evaluated on nonempty text. The blank special value is treated as one space.



## Examples

{% raw %}
```expressif
"a, b;c" | tokenize-regex("[,;] *") → {"a", "b", "c"}
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/tokenization`  
**Aliases:** None
{: .member-reference }
