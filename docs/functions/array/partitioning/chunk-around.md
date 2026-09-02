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
array →
chunk-around(
    position: integer
) → tuple
```

Separates the element at a zero-based position from the elements before and after it, returning the three parts as a tuple. Returns `null` when the position is invalid or the input cannot be evaluated.

## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `position` | `integer` | Yes | The zero-based position of the element to separate. |





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



## Examples

{% raw %}
```expressif
{10, 20, 30, 40} | chunk-around(2) → T({10, 20}, 30, {40})
```
{% endraw %}


**Kind:** Function  
**Scope:** `array/partitioning`  
**Aliases:** None
{: .member-reference }
