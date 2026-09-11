---
layout: docs
title: "recode"
parent: "Special functions"
grand_parent: "Functions library"
nav_order: 130
has_toc: false
permalink: /functions/special/recode/
tags:
  - functions
  - special
generated: true
---

```
any →
recode(
    mapping: dictionary
) → any
```

Returns the dictionary value associated with the input key, or preserves the input when no key matches.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `mapping` | `dictionary` | Yes | The dictionary supplying replacement values for unique keys. |



## Argument evaluation

- **`mapping`:** Evaluated once in the enclosing expression context. Field references read that context's record, while the value entering this recode call is the key to match.


## Behavior

Output depends on the matched dictionary value, or preserves the input and its type when absent. Matching uses the dictionary's structural key equality, including null and structured keys; an empty dictionary preserves every input. Exactly one dictionary is required; arrays of pairs, tuples, records, groupings, and null mappings are rejected.



## Examples

{% raw %}
```expressif
"A" | recode(!{("A" => 1), ("B" => 2)}) → 1
"X" | recode(!{("A" => 1)}) → "X"
{code := "A", status-codes := !{("A" => 1)}} | .code | recode(^.status-codes) → 1
```
{% endraw %}


**Kind:** Function  
**Scope:** `special`  
**Aliases:** None
{: .member-reference }
