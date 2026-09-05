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
| [`drop-null-fields`]({{ '/functions/record/drop-null-fields/' | relative_url }}) | Removes null-valued fields from the input record without traversing nested records or collections. |
| [`field`]({{ '/functions/record/field/' | relative_url }}) | Returns the value of the named field from the input record or object. Returns `null` when the field does not exist or the input does not expose named values. |
| [`put`]({{ '/functions/record/put/' | relative_url }}) | Creates or replaces statically named fields while preserving every other field. Assignment expressions are evaluated against the original input record. |
| [`put-absent`]({{ '/functions/record/put-absent/' | relative_url }}) | Assigns statically named fields only when they are absent; a present field containing null remains unchanged. |
| [`put-absent-path`]({{ '/functions/record/put-absent-path/' | relative_url }}) | Assigns the field at a dynamic path only when the final segment is absent, creating missing intermediate records. |
| [`put-path`]({{ '/functions/record/put-path/' | relative_url }}) | Creates or replaces the field at a dynamic path. Text is one literal segment; a tuple supplies nested segments, creating missing intermediate records. |
| [`put-present`]({{ '/functions/record/put-present/' | relative_url }}) | Assigns statically named fields only when they are present, including fields whose value is null. |
| [`put-present-path`]({{ '/functions/record/put-present-path/' | relative_url }}) | Assigns the field at a dynamic path only when the final segment is present, including when its value is null. |
| [`record`]({{ '/functions/record/record/' | relative_url }}) | Creates a record by evaluating its named and spread entries against the input value. Later entries overwrite fields with the same name created by earlier entries. |
| [`with`]({{ '/functions/record/with/' | relative_url }}) | Evaluates named projections independently against the input, then evaluates a body expression against their temporary record. |
