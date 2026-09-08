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
| `comparer` | `any` | No |  |

## Argument evaluation

- **`references`:** The reference collection is evaluated once in the enclosing context, then its values are compared until a match is found.
- **`comparer`:** Supplied as comparer configuration and reused during comparisons; it is not evaluated as an expression for each value.

**Kind:** Predicate  
**Scope:** `text`  
**Aliases:** `any-of`, `text-is-any-of`
{: .member-reference }
