---
layout: docs
title: "label"
parent: "Tuple functions"
grand_parent: "Functions library"
nav_order: 40
has_toc: false
permalink: /functions/tuple/label/
tags:
  - functions
  - tuple
generated: true
---

```
tuple →
label(
    ...names: text
) → record
```

Returns a flat record by assigning one positional label to each tuple item and qualifying every field expanded from a record item.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `names` | `text` | Variadic (zero or more) | One label for each tuple position, in positional order. |



## Argument evaluation

Visits each position of the tuple supplied as pipeline input to this label call in positional order. A record item contributes its fields in source order, while every other item contributes one field.

- **`names`:** Each supplied label is evaluated once in the enclosing context before the tuple positions are visited.


## Behavior

The number of labels must equal the tuple arity. A scalar or nested tuple is emitted under its positional label; a record is expanded one level and every field is named `<label>.<field>`. Nested tuples remain atomic. Tuple positions and source-record fields retain their original order. Empty tuples accept no labels and return an empty record. Only positional label arguments are accepted; spread arguments are not supported. An arity mismatch or duplicate resulting field name raises an evaluation error.



## Examples

{% raw %}
```expressif
T(42, {key := 1, value := "Brol"}, "A", {key := 2, score := 10}) | label("id", "left", "grade", "right") → {id := 42, "left.key" := 1, "left.value" := "Brol", grade := "A", "right.key" := 2, "right.score" := 10}
T(T(1, 2), {name := "Alice"}) | label("coordinates", "customer") → {coordinates := T(1, 2), "customer.name" := "Alice"}
```
{% endraw %}


**Kind:** Function  
**Scope:** `tuple`  
**Aliases:** None
{: .member-reference }
