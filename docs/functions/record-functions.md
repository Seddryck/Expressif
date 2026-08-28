---
layout: docs
title: "Record functions"
parent: "Functions library"

nav_order: 40
has_children: true
has_toc: false
permalink: /functions/record-functions/
tags:
  - functions
  - record

generated: true
---

Reference documentation for Expressif functions in the `record` scope.

| Name | Overview |
|:-----|:---------|
| [`field`]({{ '/functions/record/field/' | relative_url }}) | Returns the value of the named field from the input record or object. Returns `null` when the field does not exist or the input does not expose named values. |
| [`record`]({{ '/functions/record/record/' | relative_url }}) | Creates a record by evaluating its named and spread entries against the input value. Later entries overwrite fields with the same name created by earlier entries. |
| [`with`]({{ '/functions/record/with/' | relative_url }}) | Evaluates named projections independently against the input, then evaluates a body expression against their temporary record. |
