---
layout: docs
title: "ends-with"
parent: "Text predicates"
grand_parent: "Predicates library"
nav_order: 20
has_toc: false
permalink: /predicates/text/ends-with/
tags:
  - predicates
  - text
generated: true
---

```
ends-with(
    reference: text,
    comparer?: any
)
```

Returns `true` if the value passed as argument ends with the text value passed as parameter. Returns `false` otherwise.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `reference` | `text` | Yes | A string to be compared to the argument value.. |
| `comparer` | `any` | No | A definition of the parameters of the comparison (case-sensitivity, culture-sensitivity). |

## Argument evaluation

- **`reference`:** Evaluated once in the enclosing context unless the incoming value is null, in which case it is skipped.
- **`comparer`:** Supplied as comparer configuration and reused during comparisons; it is not evaluated as an expression for each value.

**Kind:** Predicate  
**Scope:** `text`  
**Aliases:** `text-ends-with`
{: .member-reference }
