---
layout: docs
title: "is-any-of"
parent: "Text predicates"
grand_parent: "Predicates library"
nav_order: 30
has_toc: false
permalink: /predicates/text/is-any-of/
tags:
  - predicates
  - text
generated: true
---

```
is-any-of(
    references: array,
    comparer?: any
)
```

Returns `true` if the list of text values passed as parameter contains the text value passed as argument. Returns `false` otherwise.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `references` | `array` | Yes | An array of text values. |
| `comparer` | `any` | No | Controls case and culture sensitivity. When omitted, comparison uses invariant culture and ignores case. |



## Argument evaluation

- **`references`:** The reference collection is evaluated once in the enclosing context, then its values are compared until a match is found.
- **`comparer`:** Supplied as comparer configuration and reused during comparisons; it is not evaluated as an expression for each value.



## Examples

{% raw %}
```expressif
#null | is-any-of(#null) → #false
#null | is-any-of(#null) → #false
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `text`  
**Aliases:** `any-of`, `text-is-any-of`
{: .member-reference }
