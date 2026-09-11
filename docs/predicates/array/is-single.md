---
layout: docs
title: "is-single"
parent: "Array predicates"
grand_parent: "Predicates library"
nav_order: 10
has_toc: false
permalink: /predicates/array/is-single/
tags:
  - predicates
  - array
generated: true
---

```
array →
is-single() → boolean
```

Returns whether the input array contains exactly one element. Returns false when the input cannot be evaluated as an array.



## Parameters



This predicate has no parameters.





## Behavior

Cardinality is independent of element values: a sole null, array, or record counts as one element. Uses the same array conversion as single, including text containing an Expressif array literal. When is-single returns true, single returns the sole element.



## Examples

{% raw %}
```expressif
{42} | is-single → #true
{} | is-single → #false
{1, 2} | is-single → #false
{#null} | is-single → #true
```
{% endraw %}


**Kind:** Predicate  
**Scope:** `array`  
**Aliases:** None
{: .member-reference }
