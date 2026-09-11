---
layout: docs
title: "drill-down"
parent: "Grouping functions"
grand_parent: "Functions library"
nav_order: 10
has_toc: false
permalink: /functions/grouping/drill-down/
tags:
  - functions
  - grouping
generated: true
---

```
grouping →
drill-down(
    ...expressions: expression
) → grouping
```

Refines each existing group by appending dimensions derived from its values.



## Parameters



| Name | Type | Required | Description |
|:-----|:-----|:---------|:------------|
| `expressions` | `expression` | Variadic (one or more) | One or more expressions whose results are appended to the existing key. |



## Argument evaluation

Visits each value within each group of the grouping supplied as pipeline input to this drill-down call, in parent-group and value order.

- **`expressions`:** Each supplied expression is evaluated once per grouped value, in declaration order, with that value as its context; .field reads that value's field.


## Behavior

Appends to an existing tuple key or starts a tuple with an existing scalar key. Derived tuples remain single dimensions. Structural equality includes null; parent groups, first-seen subgroups and values retain their order. Empty groups produce no subgroups. Like group-by, requires one or more positional expressions; named arguments and spread are rejected.

### Break country sales into annual revenue

The first example starts with orders grouped by country. `drill-down(.year)` splits each country group using the year on each order: Belgium becomes separate 2025 and 2026 groups, while France keeps its own 2025 group. `summarize` then adds the order amounts within each new group; the resulting keys are `(country, year)`.

### Add year and sales channel together

The second example starts with existing country groups and appends two dimensions in one call. Both `.year` and `.channel` read each order inside its current country group, producing `(country, year, channel)` keys. The country is retained from the existing key; it does not need to appear on the grouped orders.



## Examples

{% raw %}
```expressif
{
  {country := "BE", year := 2025, amount := 120},
  {country := "FR", year := 2025, amount := 90},
  {country := "BE", year := 2026, amount := 80},
  {country := "BE", year := 2025, amount := 30}
}
| group-by(.country)
| drill-down(.year)
| summarize(map(.amount) | sum)
→ !{
  (T("BE", 2025) => 150),
  (T("BE", 2026) => 80),
  (T("FR", 2025) => 90)
}

#{
  ("BE" => {
    {year := 2025, channel := "web", amount := 120},
    {year := 2025, channel := "store", amount := 30},
    {year := 2026, channel := "web", amount := 80}
  }),
  ("FR" => {
    {year := 2025, channel := "web", amount := 90}
  })
}
| drill-down(.year, .channel)
| summarize(map(.amount) | sum)
→ !{
  (T("BE", 2025, "web") => 120),
  (T("BE", 2025, "store") => 30),
  (T("BE", 2026, "web") => 80),
  (T("FR", 2025, "web") => 90)
}
```
{% endraw %}


**Kind:** Function  
**Scope:** `grouping`  
**Aliases:** None
{: .member-reference }
