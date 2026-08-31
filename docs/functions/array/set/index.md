---
layout: docs
title: "Set functions"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 60
has_children: true
has_toc: false
permalink: /functions/array/set/
tags:
  - functions
  - array
  - set
generated: true
---

Reference documentation for Expressif functions in the `array/set` scope.

| Name | Overview |
|:-----|:---------|
| [`complement`]({{ '/functions/array/set/complement/' | relative_url }}) | Returns the distinct values from the specified array that do not appear in the pipeline input, preserving the specified array order. Returns `null` when the input cannot be evaluated. |
| [`difference`]({{ '/functions/array/set/difference/' | relative_url }}) | Returns the distinct values from the pipeline input that do not appear in the specified array, preserving the pipeline input order. Returns `null` when the input cannot be evaluated. |
| [`distinct`]({{ '/functions/array/set/distinct/' | relative_url }}) | Returns the unique values from the input array in the order of their first occurrence. Returns `null` when the input cannot be evaluated. |
| [`intersection`]({{ '/functions/array/set/intersection/' | relative_url }}) | Returns the distinct values found in both the pipeline input and the specified array, preserving the pipeline input order. Returns `null` when the input cannot be evaluated. |
| [`symmetric-difference`]({{ '/functions/array/set/symmetric-difference/' | relative_url }}) | Returns the distinct values that appear in exactly one of the two arrays, listing pipeline-input exclusives first and parameter-array exclusives second while preserving order within each source. Returns `null` when the input cannot be evaluated. |
| [`union`]({{ '/functions/array/set/union/' | relative_url }}) | Returns the distinct values appearing in either the pipeline input or the specified array, listing pipeline-input values first and argument-only values second while preserving order within each source. Returns `null` when the input cannot be evaluated. |
