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
| [`first-elements`]({{ '/functions/array/selection/first-elements/' | relative_url }}) | Returns up to the requested number of elements from the start of the input enumerable. Returns `null` when the input is not an enumerable, is a string, or the count is negative. |
| [`last-elements`]({{ '/functions/array/selection/last-elements/' | relative_url }}) | Returns up to the requested number of elements from the end of the input enumerable, preserving their order. Returns `null` when the input is not an enumerable, is a string, or the count is negative. |
| [`single`]({{ '/functions/array/selection/single/' | relative_url }}) | Returns the only element of the input array without transforming it. Returns `null` when the input is empty, contains more than one element, or cannot be evaluated as an array. |
| [`skip-first-elements`]({{ '/functions/array/selection/skip-first-elements/' | relative_url }}) | Omits the requested number of elements from the start of the input enumerable and returns the remainder. Returns `null` when the input is not an enumerable, is a string, or the count is negative. |
| [`skip-last-elements`]({{ '/functions/array/selection/skip-last-elements/' | relative_url }}) | Omits the requested number of elements from the end of the input enumerable and returns the remainder. Returns `null` when the input is not an enumerable, is a string, or the count is negative. |
| [`slice-elements`]({{ '/functions/array/selection/slice-elements/' | relative_url }}) | Returns the elements in the zero-based half-open range from start, inclusive, to end, exclusive. Returns `null` when the input is not an enumerable, is a string, or either bound is negative. |
| [`value-at`]({{ '/functions/array/selection/value-at/' | relative_url }}) | Returns the input item at the specified zero-based position. Returns `null` when the position is negative or out of range, or the input cannot be evaluated. |
