---
layout: docs
title: "position-of"
parent: "Sequencing functions"
grand_parent: "Array functions"
nav_order: 50
has_toc: false
permalink: /functions/array/sequencing/position-of/
tags:
  - functions
  - array/sequencing
generated: true
---

```
array →
position-of(
    value: any
) → integer
```

Returns the zero-based position of the first input item equal to the specified value. Returns `null` when no item matches or the input cannot be evaluated.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `value` | `any` | Yes | Specifies the value to locate. |



## Structural semantics

- Cardinality: `collapsed` <span class="semantics-info" title="The visited collection produces one result." aria-label="Cardinality definition: The visited collection produces one result.">i</span>
- Dependency: `whole-input` <span class="semantics-info" title="An output depends on the complete visited input." aria-label="Dependency definition: An output depends on the complete visited input.">i</span>
- Ordering: `not-applicable` <span class="semantics-info" title="The result has no element ordering to describe." aria-label="Ordering definition: The result has no element ordering to describe.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

- **`value`:** Evaluated once in the enclosing context.



## Examples

{% raw %}
```expressif
{"a", "b", "c"} | position-of("b") → 1
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/sequencing`  
**Aliases:** `position-of`
{: .member-reference }
