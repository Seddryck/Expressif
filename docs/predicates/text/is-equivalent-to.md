---
layout: docs
title: "is-equivalent-to"
parent: "Text predicates"
grand_parent: "Predicates library"
nav_order: 80
has_toc: false
permalink: /predicates/text/is-equivalent-to/
tags:
  - predicates
  - text
generated: true
---

```
is-equivalent-to(
    reference: text,
    comparer?: any
)
```

Compare the text value passed as argument and the text value passed as parameter and returns `true` if they are equal. By default the comparison is agnostic of the culture and case-insensitive.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `reference` | `text` | Yes | A string to be compared to the argument value. |
| `comparer` | `any` | No | Controls case and culture sensitivity. When omitted, comparison uses invariant culture and ignores case. |



## Argument evaluation

- **`reference`:** Evaluated once in the enclosing context.
- **`comparer`:** Supplied as comparer configuration and reused during comparisons; it is not evaluated as an expression for each value.



## Examples

{% raw %}
```expressif
"Hello World" | is-equivalent-to("Hello") → #false
"Hello World" | is-equivalent-to("Hello") → #false
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `text`  
**Aliases:** `equivalent-to`, `text-is-equivalent-to`
{: .member-reference }
