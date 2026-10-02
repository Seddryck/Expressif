---
layout: docs
title: "chunk-around"
parent: "Partitioning functions"
grand_parent: "Array functions"
nav_order: 20
has_toc: false
permalink: /functions/array/partitioning/chunk-around/
tags:
  - functions
  - array/partitioning
generated: true
---

```
array<T> →
chunk-around(
    position: integer
) → tuple<array<T>, T, array<T>>
```

Separates the element at a zero-based position from the elements before and after it, returning the three parts as a tuple. Returns `null` when the position is invalid or the input cannot be evaluated.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `position` | `integer` | Yes | The zero-based position of the element to separate. |



## Examples

{% raw %}
```expressif
{10, 20, 30, 40} | chunk-around(2) → T({10, 20}, 30, {40})
```
{% endraw %}

## Value shape

- Pipeline input: `array<T>`
- Returns: `tuple<array<T>, T, array<T>>`
- Nullability: The result is nullable when the pipeline input is nullable.

`T`, `U`, and other capital letters represent related value shapes. Repeated letters refer to the same shape within the contract.


## Structural semantics

- Cardinality: `partitioned` <span class="semantics-info" title="Visited inputs are reorganized into groups or partitions." aria-label="Cardinality definition: Visited inputs are reorganized into groups or partitions.">i</span>
- Dependency: `partition` <span class="semantics-info" title="An output depends on the elements belonging to the same partition or key." aria-label="Dependency definition: An output depends on the elements belonging to the same partition or key.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

- **`position`:** Evaluated once in the enclosing context.


## Behavior

`chunk-around` materializes the input and returns `T(before, selected, after)`. The selected value is preserved as a scalar, including when it is `null` or structured. A position is valid only when it identifies an existing element.

A common use is resuming an approval workflow where the item at the cursor has a distinct role. Given these steps:

```expressif
{ValidateInvoice, ManagerApproval, FinanceApproval, ReleasePayment}
```

and a current position of `2`, the required structure is `completed steps | current step | future steps`:

```expressif
steps | chunk-around(2)
→ T(
    {ValidateInvoice, ManagerApproval},
    FinanceApproval,
    {ReleasePayment}
)
```

`FinanceApproval` remains a single workflow-step record, so it can be displayed, executed, or updated as the current step. By contrast, `chunk-on(2)` returns only the values before the cursor and the values from the cursor onward:

```expressif
steps | chunk-on(2)
→ T(
    {ValidateInvoice, ManagerApproval},
    {FinanceApproval, ReleasePayment}
)
```

The same three-role structure—`past | selected/current item | future`—appears in workflow engines, carousel focus, undo/redo histories, breadcrumb navigation, and processing a specific failed event in a sequence. The equivalent result can be constructed by splitting the right chunk again, but `chunk-on` alone does not distinguish the current item from future items. `chunk-around` directly provides this array-zipper operation.

**Kind:** Function  
**Scope:** `array/partitioning`  
**Aliases:** None
{: .member-reference }
