---
layout: docs
title: "Sequencing functions"
parent: "Array functions"
grand_parent: "Functions library"
nav_order: 50
has_children: true
has_toc: false
permalink: /functions/array/sequencing/
tags:
  - functions
  - array
  - sequencing
generated: true
---

Reference documentation for Expressif functions in the `array/sequencing` scope.

| Name | Overview |
|:-----|:---------|
| [`adjacent`]({{ '/functions/array/sequencing/adjacent/' | relative_url }}) | Evaluates an operation against every consecutive pair of input values. Returns `null` when the input cannot be evaluated. |
| [`lag`]({{ '/functions/array/sequencing/lag/' | relative_url }}) | Returns the previous value for each input element. The first output value is `null` because there is no previous element. Preserves input cardinality (one output item per input item). Returns `null` when the input is not an enumerable or is a string. |
| [`lead`]({{ '/functions/array/sequencing/lead/' | relative_url }}) | Returns the next value for each input element. The last output value is `null` because there is no next element. Preserves input cardinality (one output item per input item). Returns `null` when the input is not an enumerable or is a string. |
| [`pairwise`]({{ '/functions/array/sequencing/pairwise/' | relative_url }}) | Returns each consecutive pair of input values as a tuple. Returns `null` when the input cannot be evaluated. |
| [`position-of`]({{ '/functions/array/sequencing/position-of/' | relative_url }}) | Returns the zero-based position of the first input item equal to the specified value. Returns `null` when no item matches or the input cannot be evaluated. |
| [`reverse`]({{ '/functions/array/sequencing/reverse/' | relative_url }}) | Returns the input enumerable with elements emitted in the opposite order. Preserves input cardinality (one output item per input item). Returns `null` when the input is not an enumerable or is a string. |
| [`with-position`]({{ '/functions/array/sequencing/with-position/' | relative_url }}) | Returns each input item paired with its zero-based position as a tuple in `(position, value)` order. Preserves input order and cardinality. Position terminology distinguishes sequence locations from indexes used to accelerate searches. Returns `null` when the input cannot be evaluated. |
