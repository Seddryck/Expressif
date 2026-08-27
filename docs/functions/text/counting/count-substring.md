---
layout: docs
title: "count-substring"
parent: "Counting functions"
grand_parent: "Text functions"
nav_order: 20
has_toc: false
permalink: /functions/text/counting/count-substring/
tags:
  - functions
  - text/counting
generated: true
---

```
text →
count-substring(
    substring: text
) → integer
```

Returns the count of non-overlapping occurrences of a substring, defined as a parameter, in the argument value.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `substring` | `text` | Yes | The substring to count in the argument value. |





## Examples

```expressif
"Hello World" | count-substring("lo") → 1
```


**Kind:** Function  
**Scope:** `text/counting`  
**Aliases:** `text-to-count-substring`
{: .member-reference }
