---
layout: docs
title: "is-sorted-after"
parent: "Text predicates"
grand_parent: "Predicates library"
nav_order: 80
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
| `comparer` | `any` | No | A definition of the parameters of the comparison (case-sensitivity, culture-sensitivity). |

## Argument evaluation

- **`reference`:** Evaluated in the enclosing context for special-value checks and for the comparison; the reference can be read more than once.
- **`comparer`:** Supplied as comparer configuration and reused during comparisons; it is not evaluated as an expression for each value.

**Kind:** Predicate  
**Scope:** `text`  
**Aliases:** `sorted-after`, `text-is-sorted-after`
{: .member-reference }
