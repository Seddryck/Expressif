---
layout: docs
title: "expand"
parent: "Record functions"
grand_parent: "Functions library"
nav_order: 30
has_toc: false
permalink: /functions/record/expand/
tags:
  - functions
  - record
generated: true
---

```
record →
expand(
    selector: expression,
    label?: text
) → record
```

Flattens a selected nested record into its parent, qualifying conflicts or every expanded field when a label is supplied.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `selector` | `expression` | Yes | An expression selecting the nested record to expand. |
| `label` | `text` | No | An optional qualifier for every expanded field. Omission derives the qualifier from a direct field selector and qualifies only conflicts. |



## Argument evaluation

- **`selector`:** Evaluated once with the record supplied as pipeline input to this expand call as its context. A direct .field reads that record, including changes made by earlier pipeline stages.
- **`label`:** When supplied, evaluated after the selector with the record entering this expand call as its context, before expanding its fields.


## Behavior

Without a label, the selector must be a direct field selector such as .customer; computed selectors require a label at binding time. A direct selector consumes its field; a computed selector beginning with a direct field selection consumes that first field, while other computed selectors append their expanded fields. Expanded fields replace the consumed field in nested field order; all remaining parent fields retain their order. Expansion is shallow. Null or missing selections add no fields; non-record selections return null. Non-record pipeline input is rejected. Field names are case-sensitive. Parent fields are authoritative: conflicting expanded names receive the qualifier repeatedly until unique, while unique unqualified nested names are reserved before resolving conflicts. Explicit labels qualify every field, including when the label is empty. See [label](/functions/tuple/label/) and [label-conflicts](/functions/tuple/label-conflicts/) for symmetric tuple shaping.



## Examples

{% raw %}
```expressif
{id := 1, name := "Order", customer := {name := "Alice", country := "BE"}} | expand(.customer) → {id := 1, name := "Order", "customer.name" := "Alice", country := "BE"}
{customer := {name := "Alice"}} | expand(.customer, "buyer") → {"buyer.name" := "Alice"}
{customer := {address := {country := "BE"}}} | expand(.customer) | expand(.address) → {country := "BE"}
```
{% endraw %}


**Kind:** Function  
**Scope:** `record`  
**Aliases:** None
{: .member-reference }
