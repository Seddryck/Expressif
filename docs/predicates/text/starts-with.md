---
layout: docs
title: "starts-with"
parent: "Text predicates"
grand_parent: "Predicates library"
nav_order: 180
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
| `comparer` | `any` | No | A definition of the parameters of the comparison (case-sensitivity, culture-sensitivity). |

## Argument evaluation

- **`reference`:** Evaluated in the enclosing context for special-value checks and for the comparison; the reference can be read more than once.
- **`comparer`:** Supplied as comparer configuration and reused during comparisons; it is not evaluated as an expression for each value.

**Kind:** Predicate  
**Scope:** `text`  
**Aliases:** `text-starts-with`
{: .member-reference }
