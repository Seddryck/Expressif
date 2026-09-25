---
layout: docs
title: "let"
parent: "Flow functions"
grand_parent: "Functions library"
nav_order: 40
has_toc: false
permalink: /functions/flow/let/
tags:
  - functions
  - flow
generated: true
---

```
any →
let(
    ...bindings: entry
) → any
```

Evaluates named bindings once and preserves the pipeline input for subsequent stages.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `bindings` | `entry` | Variadic (one or more); no spread | One or more named expressions whose results become lexical values. |



## Argument evaluation

- **`bindings`:** Evaluated once in declaration order against the original input supplied to this let call, stopping on failure. All expressions use the enclosing named-value environment; bindings from this call become visible together after every expression succeeds.


## Behavior

Returns the original input unchanged, preserving its type. Reference computed values with @name, including null values. Bindings remain visible through subsequent pipeline stages and grouping parentheses in this expression invocation; nested inline invocations inherit them, may shadow them, and restore outer values on success or failure. This call adds no expression-root scope and does not change caret depth. Consecutive let calls support dependent bindings. At least one unquoted identifier assignment is required; duplicate names, positional entries, and spread are rejected. Unknown references use the normal unresolved-variable diagnostic. Unlike :>, which captures invocation input, let captures computed values; unlike with, it neither constructs a temporary record nor invokes a body. Bindings may shadow context variables without mutating them. Independently invoked expressions retain their own environment; caller values must be passed explicitly to named expressions when that invocation feature is available.



## Examples

{% raw %}
```expressif
10 | let(double := multiply(2)) | add(1) → 11
10 | let(double := multiply(2)) | add(@double) → 30
{quantity := 4, unit-price := 5} | let(total := .quantity | multiply(.unit-price)) | @total → 20
```
{% endraw %}


**Kind:** Function  
**Scope:** `flow`  
**Aliases:** None
{: .member-reference }
