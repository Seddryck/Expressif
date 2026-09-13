---
layout: docs
title: "rename-fields"
parent: "Record functions"
grand_parent: "Functions library"
nav_order: 140
has_toc: false
permalink: /functions/record/rename-fields/
tags:
  - functions
  - record
generated: true
---

```
record →
rename-fields(
    transform: expression,
    filter?: predicate
) → record
```

Transforms selected field names while preserving field values and order. Duplicate resulting names cause an evaluation error.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `transform` | `expression` | Yes | An expression transforming a field name from text to text. |
| `filter` | `predicate` | No | An optional predicate selecting field names to transform. Omission selects every field. |



## Argument evaluation

Visits the original field names of the record supplied as pipeline input to this rename-fields call, in field order. Argument expressions use the field name as their context, so .field does not read the surrounding record.

- **`transform`:** Evaluated once for each visited field name accepted by the filter, after the filter runs, with that original name as its context. Without a filter, it runs for every name; field values are never supplied.
- **`filter`:** When supplied, evaluated once per visited original field name, with that name as its context. A false result preserves the original name and skips the transformation.



## Examples

{% raw %}
```expressif
{first-name := "John", last-name := "Doe"} | rename-fields(tokenize-kebab | pascal-case) → {FirstName := "John", LastName := "Doe"}
{first-name := "John", age := 42} | rename-fields(tokenize-kebab | pascal-case, contains("-")) → {FirstName := "John", age := 42}
{" First Name " := "John"} | rename-fields(trim | lower) → {"first name" := "John"}
```
{% endraw %}


**Kind:** Function  
**Scope:** `record`  
**Aliases:** None
{: .member-reference }
