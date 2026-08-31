---
layout: docs
title: "Partitioning functions"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 30
has_children: true
has_toc: false
permalink: /functions/array/partitioning/
tags:
  - functions
  - array
  - partitioning
generated: true
---

Reference documentation for Expressif functions in the `array/partitioning` scope.

| Name | Overview |
|:-----|:---------|
| [`chunk`]({{ '/functions/array/partitioning/chunk/' | relative_url }}) | Splits an array into consecutive, non-overlapping chunks of at most the specified size, preserving a final partial chunk. It resembles a count-based tumbling window but, unlike general sliding or hopping windows, has no separate step and always keeps the final partial chunk. It does not group items by inactivity or time. Returns `null` when the input cannot be evaluated. |
| [`chunk-around`]({{ '/functions/array/partitioning/chunk-around/' | relative_url }}) | Separates the element at a zero-based position from the elements before and after it, returning the three parts as a tuple. Returns `null` when the position is invalid or the input cannot be evaluated. |
| [`chunk-on`]({{ '/functions/array/partitioning/chunk-on/' | relative_url }}) | Splits an array on a zero-based boundary and returns the elements before and from that position as a tuple. Returns `null` when the position is invalid or the input cannot be evaluated. |
| [`chunk-while`]({{ '/functions/array/partitioning/chunk-while/' | relative_url }}) | Groups consecutive values while an operation over each previous and current pair evaluates to `true`. Returns `null` when the operation does not produce a Boolean value or the input cannot be evaluated. |
