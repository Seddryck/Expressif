---
layout: docs
title: "is-sorted-after"
parent: "Text predicates"
grand_parent: "Predicates library"
nav_order: 130
has_toc: false
permalink: /predicates/text/is-sorted-after/
tags:
  - predicates
  - text
generated: true
---

```
is-sorted-after(
    reference: text,
    comparer?: any
)
```

Compare the text value passed as argument and the text value passed as parameter and returns `true` if argument value is alphabetically sorted after the parameter value. By default the comparison is agnostic of the culture and case-insensitive.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `reference` | `text` | Yes | A string to be compared to the argument value. |
| `comparer` | `any` | No | Controls case and culture sensitivity. When omitted, comparison uses invariant culture and ignores case. |



## Argument evaluation

- **`reference`:** Evaluated once in the enclosing context unless the incoming value is null, in which case it is skipped.
- **`comparer`:** Supplied as comparer configuration and reused during comparisons; it is not evaluated as an expression for each value.



## Examples

{% raw %}
```expressif
"Hello World" | is-sorted-after("Hello") → #true
"Hello World" | is-sorted-after("Hello") → #true
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `text`  
**Aliases:** `sorted-after`, `text-is-sorted-after`
{: .member-reference }
