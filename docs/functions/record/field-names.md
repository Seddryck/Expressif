---
layout: docs
title: "field-names"
parent: "Record functions"
grand_parent: "Functions library"
nav_order: 70
has_toc: false
permalink: /functions/record/field-names/
tags:
  - functions
  - record
generated: true
---

```
record →
field-names() → array
```

Returns the names of all fields in the input record, preserving field order.



## Parameters



This function has no parameters.



## Examples

{% raw %}
```expressif
{name := "Mons", temp := 18, pressure := 1012} | field-names → {"name", "temp", "pressure"}
```
{% endraw %}

## Structural semantics

- Cardinality: `preserved` <span class="semantics-info" title="The output contains the same number of elements as the visited input." aria-label="Cardinality definition: The output contains the same number of elements as the visited input.">i</span>
- Dependency: `per-element` <span class="semantics-info" title="An output element depends only on its corresponding visited input element." aria-label="Dependency definition: An output element depends only on its corresponding visited input element.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.

**Kind:** Function  
**Scope:** `record`  
**Aliases:** None
{: .member-reference }
