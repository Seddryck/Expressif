---
layout: docs
title: "filter"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 30
has_toc: false
permalink: /functions/array/filter/
tags:
  - functions
  - array
generated: true
---

```
array →
filter(
    predicate: predicate
) → array
```

Applies a predicate expression to each input item and returns only items for which the predicate evaluates to `true`. Returns `null` when the input is not an enumerable or is a string.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `predicate` | `predicate` | Yes | Expression defining the predicate applied to each input item. |



## Argument evaluation

Visits each element of the array or each pair of the dictionary supplied as pipeline input to this filter call, in enumeration order.

- **`predicate`:** Evaluated once per visited element, with that element as its context. For dictionary input, $key and $value resolve to the key and value of the visited pair.


## Behavior

**Argument form — `predicate`:** A predicate call, such as `greater-than(1)`, or an open expression that produces a Boolean, such as `.active`. A bare Boolean literal such as `#true` is not accepted.

**Element binding:** The predicate runs once for each element of the array supplied as pipeline input to this `filter` call. At the start of the predicate, `.field` reads that element's field. In `.lines | filter(.active)`, the inner predicate receives a line from the `.lines` array, not the surrounding record.

**Dictionary traversal:** A dictionary supplies its pairs directly, in dictionary order. The predicate receives each pair as its context; $key and $value access that pair. The result is an ordinary array of unchanged selected pairs. An empty dictionary produces an empty array.



## Examples

{% raw %}
```expressif
{1, 2, 3} | filter(greater-than(1)) → {2, 3}
{{active:=#true}, {active:=#false}} | filter(.active) → {{active:=#true}}
!{"BE" => 100, "FR" => 80} | filter($value | greater-than(90)) → {("BE" => 100)}
```
{% endraw %}


**Kind:** Function  
**Scope:** `array`  
**Aliases:** `filter`
{: .member-reference }
