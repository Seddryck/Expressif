---
layout: docs
title: "from-pairs"
parent: "Record functions"
grand_parent: "Functions library"
nav_order: 80
has_toc: false
permalink: /functions/record/from-pairs/
tags:
  - functions
  - record
generated: true
---

```
array →
from-pairs() → record
```

Converts an array of pairs to a record, coercing keys to text and preserving values and order.



## Parameters



This function has no parameters.



## Structural semantics

- Cardinality: `non-increasing` <span class="semantics-info" title="The output contains no more elements than the visited input." aria-label="Cardinality definition: The output contains no more elements than the visited input.">i</span>
- Dependency: `whole-input` <span class="semantics-info" title="An output depends on the complete visited input." aria-label="Dependency definition: An output depends on the complete visited input.">i</span>
- Ordering: `unordered` <span class="semantics-info" title="Output order is not part of the semantic contract." aria-label="Ordering definition: Output order is not part of the semantic contract.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.


## Argument evaluation

Visits each element of the array supplied as pipeline input to this from-pairs call, in enumeration order.



## Behavior

Every element must be a pair; ordinary two-element tuples and key/value records are rejected. Keys must coerce to text, and duplicate field names after coercion are rejected using case-sensitive comparison. Values, including null, are preserved without coercion. All text field names are supported, including empty and whitespace names. Leading underscores preserve private visibility. An empty array produces an empty record. Non-enumerable input is rejected.



## Examples

{% raw %}
```expressif
{("foo" => 1), ("bar" => 2)} | from-pairs → {foo := 1, bar := 2}
{(2025 => 100), (2026 => 120)} | from-pairs → {"2025" := 100, "2026" := 120}
```
{% endraw %}


**Kind:** Function  
**Scope:** `record`  
**Aliases:** None
{: .member-reference }
