---
layout: docs
title: "Array functions"
parent: "Functions library"

nav_order: 10
has_children: true
has_toc: false
permalink: /functions/array-functions/
tags:
  - functions
  - array

generated: true
---

Reference documentation for Expressif functions in the `array` scope.

| Name | Overview |
|:-----|:---------|
| [`map-over`]({{ '/functions/array/map-over/' | relative_url }}) | Evaluates an expression once for every supplied value while preserving the pipeline input as the expression input. Tuple values are expanded into positional arguments for a bare callable. Returns `null` when values is not enumerable or is text. |
| [`map-with`]({{ '/functions/array/map-with/' | relative_url }}) | Evaluates an expression once for every supplied value, using that value as the pipeline input and the outer input as its argument. Tuple values remain ordinary pipeline values and are not expanded. Returns `null` when values is not enumerable or is text. |
| [`zip`]({{ '/functions/array/combination/zip/' | relative_url }}) | Combines corresponding values from the input array and a second array into two-element tuples, stopping when either array is exhausted. Returns `null` when either value cannot be evaluated as an array. |
| [`zip-cycle`]({{ '/functions/array/combination/zip-cycle/' | relative_url }}) | Combines values from two arrays into two-element tuples until the longer array is exhausted, cycling each non-empty shorter array from its beginning. Returns an empty array when both inputs are empty and `null` when exactly one input is empty or either value cannot be evaluated as an array. |
| [`zip-padded`]({{ '/functions/array/combination/zip-padded/' | relative_url }}) | Combines corresponding values from the input array and a second array into two-element tuples until both arrays are exhausted, using `null` for a missing value. Returns `null` when either value cannot be evaluated as an array. |
| [`zip-strict`]({{ '/functions/array/combination/zip-strict/' | relative_url }}) | Combines corresponding values from equally sized input and parameter arrays into two-element tuples. Returns `null` when the arrays have different lengths or either value cannot be evaluated as an array. |
