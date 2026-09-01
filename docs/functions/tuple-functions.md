---
layout: docs
title: "Tuple functions"
parent: "Functions library"

nav_order: 20
has_children: true
has_toc: false
permalink: /functions/tuple-functions/
tags:
  - functions
  - tuple

generated: true
---

Reference documentation for Expressif functions in the `tuple` scope.

| Name | Overview |
|:-----|:---------|
| [`arity`]({{ '/functions/tuple/arity/' | relative_url }}) | Returns the number of positional elements in the input tuple. |
| [`extend`]({{ '/functions/tuple/extend/' | relative_url }}) | Returns a new tuple with a value appended, expanding tuple values into their positions. |
| [`pick`]({{ '/functions/tuple/pick/' | relative_url }}) | Returns a tuple containing selected positions in the requested order. |
| [`swap`]({{ '/functions/tuple/swap/' | relative_url }}) | Returns a tuple with two positions exchanged, defaulting to the first and last positions. |
| [`tuple`]({{ '/functions/tuple/tuple/' | relative_url }}) | Constructs a new tuple by evaluating zero or more positional expressions from left to right against the same input. Spread arguments expand array values in place. |
| [`tuple-at`]({{ '/functions/tuple/tuple-at/' | relative_url }}) | Returns the tuple field at the specified zero-based position. Returns `null` when the input is not a tuple or the position is out of range. |
| [`tuple-first`]({{ '/functions/tuple/tuple-first/' | relative_url }}) | Returns the first field of a tuple. Returns `null` when the input is not a tuple. |
| [`tuple-second`]({{ '/functions/tuple/tuple-second/' | relative_url }}) | Returns the second field of a tuple. Returns `null` when the input is not a tuple. |
