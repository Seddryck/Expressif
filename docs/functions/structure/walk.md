---
layout: docs
title: "walk"
parent: "Structure functions"
grand_parent: "Functions library"
nav_order: 10
has_toc: false
permalink: /functions/structure/walk/
tags:
  - functions
  - structure
generated: true
---

```
any →
walk(
    transformation: expression
) → any
```

Recursively traverses arrays, tuples, and records and evaluates an expression against each leaf value while preserving container shape.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `transformation` | `expression` | Yes | Expression evaluated against every leaf value. |



## Examples

{% raw %}
```expressif
T(42, " 42 ") | walk(trim) → T("42", "42")
T(42, " 42 ") | walk(*trim) → T(42, "42")
{name := " Bob ", address := {city := " Brussels "}} | walk(*trim) → {name := "Bob", address := {city := "Brussels"}}
```
{% endraw %}

## Structural semantics

- Cardinality: `preserved` <span class="semantics-info" title="The output contains the same number of elements as the visited input." aria-label="Cardinality definition: The output contains the same number of elements as the visited input.">i</span>
- Dependency: `per-element` <span class="semantics-info" title="An output element depends only on its corresponding visited input element." aria-label="Dependency definition: An output element depends only on its corresponding visited input element.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

Recursively visits the leaf values in the incoming arrays, tuples, and records.

- **`transformation`:** Evaluated once against each leaf reached in the incoming structure.


## Behavior

`walk` recursively visits array items, tuple items, and record field values. Record field names and container kinds are preserved, and only leaves are supplied to the transformation. The transformation retains its ordinary semantics, so `walk(trim)` permits normal coercion while `walk(*trim)` uses guarded entry.

**Kind:** Function  
**Scope:** `structure`  
**Aliases:** `walk`
{: .member-reference }
