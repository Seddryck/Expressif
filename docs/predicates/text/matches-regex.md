---
layout: docs
title: "matches-regex"
parent: "Text predicates"
grand_parent: "Predicates library"
nav_order: 220
has_toc: false
permalink: /predicates/text/matches-regex/
tags:
  - predicates
  - text
generated: true
---

```
matches-regex(
    regex: text,
    comparer?: any
)
```

Returns `true` if the value passed as argument validate the regex passed as parameter. Returns `false` otherwise.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `regex` | `text` | Yes | A string to be compared to the argument value. |
| `comparer` | `any` | No | Controls case and culture sensitivity. When omitted, comparison uses invariant culture and ignores case. |



## Argument evaluation

- **`regex`:** Evaluated once in the enclosing context unless the incoming value is null, in which case it is skipped.
- **`comparer`:** Supplied as comparer configuration and reused during comparisons; it is not evaluated as an expression for each value.



## Examples

{% raw %}
```expressif
"Hello World" | matches-regex("^Hello") → #true
"Hello World" | matches-regex("^Hello") → #true
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `text`  
**Aliases:** `text-matches-regex`
{: .member-reference }
