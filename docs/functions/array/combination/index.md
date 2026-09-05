---
layout: docs
title: "Combination functions"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 10
has_children: true
has_toc: false
permalink: /functions/array/combination/
tags:
  - functions
  - array
  - combination
generated: true
---

Reference documentation for Expressif functions in the `array/combination` scope.

| Name | Overview |
|:-----|:---------|
| [`zip`]({{ '/functions/array/combination/zip/' | relative_url }}) | Combines corresponding values from the input array and a second array into two-element tuples, stopping when either array is exhausted. Returns `null` when either value cannot be evaluated as an array. |
| [`zip-cycle`]({{ '/functions/array/combination/zip-cycle/' | relative_url }}) | Combines values from two arrays into two-element tuples until the longer array is exhausted, cycling each non-empty shorter array from its beginning. Returns an empty array when either input is empty and `null` when either value cannot be evaluated as an array. |
| [`zip-padded`]({{ '/functions/array/combination/zip-padded/' | relative_url }}) | Combines corresponding values from the input array and a second array into two-element tuples until both arrays are exhausted, using `null` for a missing value. Returns `null` when either value cannot be evaluated as an array. |
| [`zip-strict`]({{ '/functions/array/combination/zip-strict/' | relative_url }}) | Combines corresponding values from equally sized input and parameter arrays into two-element tuples. Returns `null` when the arrays have different lengths or either value cannot be evaluated as an array. |
