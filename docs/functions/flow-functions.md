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
| [`guard`]({{ '/functions/flow/guard/' | relative_url }}) | Evaluates an expression only when the current input is directly compatible with its entry contract; otherwise, returns the original input unchanged. |
| [`transform-as`]({{ '/functions/flow/transform-as/' | relative_url }}) | Transforms one or more named expression results with the same open expression and returns them as a record. |
| [`transform-with`]({{ '/functions/flow/transform-with/' | relative_url }}) | Transforms the results of one or more expressions with the same open expression and returns them as a tuple. |
