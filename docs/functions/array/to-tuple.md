---
layout: docs
title: "to-tuple"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 200
has_toc: false
permalink: /functions/array/to-tuple/
tags:
  - functions
  - array
generated: true
---

```
array →
to-tuple() → tuple
```

Returns a tuple containing the input array's elements in order. Returns `null` when the input is not an array.

## Parameters



This function has no parameters.





## Behavior

`to-tuple` materializes the input array as a tuple without changing its elements. Null values and nested arrays, records, and tuples are preserved without recursive conversion.



## Examples

{% raw %}
```expressif
{1, "A", #true} | to-tuple → T(1, "A", #true)
{} | to-tuple → T()
```
{% endraw %}


**Kind:** Function  
**Scope:** `array`  
**Aliases:** None
{: .member-reference }
