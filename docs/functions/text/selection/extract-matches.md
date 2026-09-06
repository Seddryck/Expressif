---
layout: docs
title: "extract-matches"
parent: "Selection functions"
grand_parent: "Text functions"
nav_order: 30
has_toc: false
permalink: /functions/text/selection/extract-matches/
tags:
  - functions
  - text/selection
generated: true
---

```
text →
extract-matches(
    pattern: text
) → array
```

Returns complete non-overlapping regular expression matches in source order, including zero-length matches. Returns an empty array for null or empty input or when no match is found.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `pattern` | `text` | Yes | The .NET regular expression identifying matches. Matching is case-sensitive unless inline options specify otherwise. |





## Behavior

Returns whole matches without adding capture groups. Invalid patterns raise a regular expression error when evaluated on nonempty text. Matching uses a one-second timeout and raises RegexMatchTimeoutException when the limit is exceeded. The blank special value is treated as one space.



## Examples

{% raw %}
```expressif
"a12b34" | extract-matches("[0-9]+") → {"12", "34"}
```
{% endraw %}


**Kind:** Function  
**Scope:** `text/selection`  
**Aliases:** None
{: .member-reference }
