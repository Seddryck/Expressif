---
layout: docs
title: "Flow functions"
parent: "Functions library"

nav_order: 10
has_children: true
has_toc: false
permalink: /functions/flow-functions/
tags:
  - functions
  - flow

generated: true
---

Reference documentation for Expressif functions in the `flow` scope.

| Name | Overview |
|:-----|:---------|
| [`apply`]({{ '/functions/flow/apply/' | relative_url }}) | Evaluates an expression with the input value as its current context. |
| [`catch`]({{ '/functions/flow/catch/' | relative_url }}) | Returns a recovery result and terminates the current pipeline when the input is null; otherwise, passes the input through. |
| [`guard`]({{ '/functions/flow/guard/' | relative_url }}) | Evaluates an expression only when the current input is directly compatible with its entry contract; otherwise, returns the original input unchanged. |
| [`switch`]({{ '/functions/flow/switch/' | relative_url }}) | Returns the result of the first branch whose predicate accepts the original input, or the final fallback, or null. |
| [`throw`]({{ '/functions/flow/throw/' | relative_url }}) | Raises an evaluation exception when the input is rejected; otherwise, passes the input through. |
| [`transform-as`]({{ '/functions/flow/transform-as/' | relative_url }}) | Transforms one or more named expression results with the same open expression and returns them as a record. |
| [`transform-with`]({{ '/functions/flow/transform-with/' | relative_url }}) | Transforms the results of one or more expressions with the same open expression and returns them as a tuple. |
| [`try`]({{ '/functions/flow/try/' | relative_url }}) | Returns the first candidate result accepted by its predicate, or the final fallback, or null. |
