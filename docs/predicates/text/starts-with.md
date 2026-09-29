---
layout: docs
title: "starts-with"
parent: "Text predicates"
grand_parent: "Predicates library"
nav_order: 240
has_toc: false
permalink: /predicates/text/starts-with/
tags:
  - predicates
  - text
generated: true
---

```
starts-with(
    reference: text,
    comparer?: any
)
```

Returns `true` if the value passed as argument starts with the text value passed as parameter. Returns `false` otherwise.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `reference` | `text` | Yes | A string to be compared to the argument value.. |
| `comparer` | `any` | No | Controls case and culture sensitivity. When omitted, comparison uses invariant culture and ignores case. |



## Argument evaluation

- **`reference`:** Evaluated once in the enclosing context unless the incoming value is null, in which case it is skipped.
- **`comparer`:** Supplied as comparer configuration and reused during comparisons; it is not evaluated as an expression for each value.



## Examples

{% raw %}
```expressif
"Hello World" | starts-with("Hello") → #true
"Hello World" | starts-with("Hello") → #true
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `text`  
**Aliases:** `text-starts-with`
{: .member-reference }
