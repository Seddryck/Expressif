---
layout: docs
title: "pairs"
parent: "Record functions"
grand_parent: "Functions library"
nav_order: 120
has_toc: false
permalink: /functions/record/pairs/
tags:
  - functions
  - record
generated: true
---

```
record →
pairs() → array
```

Converts all record fields to pairs, preserving field names, values, and order.



## Parameters



This function has no parameters.



## Structural semantics

- Cardinality: `preserved` <span class="semantics-info" title="The output contains the same number of elements as the visited input." aria-label="Cardinality definition: The output contains the same number of elements as the visited input.">i</span>
- Dependency: `per-element` <span class="semantics-info" title="An output element depends only on its corresponding visited input element." aria-label="Dependency definition: An output element depends only on its corresponding visited input element.">i</span>
- Ordering: `preserved` <span class="semantics-info" title="Relative source order is retained." aria-label="Ordering definition: Relative source order is retained.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

Visits every field of the record supplied as pipeline input to this pairs call, in field order, including private fields.



## Behavior

Each pair has the original field name as its key and the unchanged field value as its value, including null. An empty record produces an empty array. Non-record input is rejected.



## Examples

{% raw %}
```expressif
{foo := 1, bar := 2} | pairs → {("foo" => 1), ("bar" => 2)}
{_id := 42, name := "Cedric"} | pairs | from-pairs → {_id := 42, name := "Cedric"}
```
{% endraw %}


**Kind:** Function  
**Scope:** `record`  
**Aliases:** None
{: .member-reference }
