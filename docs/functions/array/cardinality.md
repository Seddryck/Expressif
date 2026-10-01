---
layout: docs
title: "cardinality"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 20
has_toc: false
permalink: /functions/array/cardinality/
tags:
  - functions
  - array
generated: true
---

```
array →
cardinality() → integer
```

Returns the number of elements in the input array.



## Parameters



This function has no parameters.



## Structural semantics

- Cardinality: `collapsed` <span class="semantics-info" title="The visited collection produces one result." aria-label="Cardinality definition: The visited collection produces one result.">i</span>
- Dependency: `whole-input` <span class="semantics-info" title="An output depends on the complete visited input." aria-label="Dependency definition: An output depends on the complete visited input.">i</span>
- Ordering: `not-applicable` <span class="semantics-info" title="The result has no element ordering to describe." aria-label="Ordering definition: The result has no element ordering to describe.">i</span>

See [Structural semantics](/Expressif/language/structural-semantics/) for the definitions and their relationship to traversal and argument evaluation.





## Examples

{% raw %}
```expressif
{} | cardinality → 0
{1, 2, 3} | cardinality → 3
```
{% endraw %}


**Kind:** Function  
**Scope:** `array`  
**Aliases:** None
{: .member-reference }
