---
title: Groups in practice
parent: Expressif language
nav_order: 13
description: Group records by a key, transform or summarize each group, filter whole groups, and reuse array operations deliberately.
---

# Groups in practice

Groupings are useful when values need to stay together under a shared key. A common example is dividing orders by country before transforming, aggregating, or filtering those country-level buckets.

This page uses the following orders throughout:

{% raw %}
```expressif
{
  {country := "BE", customer := "Alice", amount := 120},
  {country := "FR", customer := "Claire", amount := 90},
  {country := "BE", customer := "Bob", amount := 30},
  {country := "BE", customer := "Alice", amount := 80}
}
```
{% endraw %}

## Creating a grouping

[`group-by`]({{ '/functions/array/grouping/group-by/' | relative_url }}) evaluates its key expression for every array element. Values with equal keys are collected into the same group:

{% raw %}
```expressif
{
  {country := "BE", customer := "Alice", amount := 120},
  {country := "FR", customer := "Claire", amount := 90},
  {country := "BE", customer := "Bob", amount := 30},
  {country := "BE", customer := "Alice", amount := 80}
}
| group-by(.country)
```
{% endraw %}

The result is:

{% raw %}
```expressif
#{
  ("BE" => {
    {country := "BE", customer := "Alice", amount := 120},
    {country := "BE", customer := "Bob", amount := 30},
    {country := "BE", customer := "Alice", amount := 80}
  }),
  ("FR" => {
    {country := "FR", customer := "Claire", amount := 90}
  })
}
```
{% endraw %}

Each entry is a group with a key and a collection of grouped values. In a context representing one group, `$key` reads its key and `$value` reads its complete value collection.

### A grouping is dictionary-like

A grouping is an ordered, dictionary-like structure whose keys are unique and whose value at each key is a collection. The literal syntax `#{...}` distinguishes it from an array (`{...}`) and a dictionary (`!{...}`).

The key uniqueness has practical consequences:

- `group-by` merges all values with structurally equal keys into one group; it does not create repeated `"BE"` entries.
- Constructing a grouping literal or calling `grouping(...)` with the same key more than once is invalid. Put all values for that key in one collection instead.
- `map-groups` and `filter-groups` preserve existing keys. They transform or select groups without creating ambiguous duplicate keys.
- When one group is the current context, use `$key` for the unique key and `$value` for its collection. For example, `filter-groups($key | is-equivalent-to("BE"))` selects the Belgian group.

The groups keep the order in which their keys first occur. Values inside each group keep their original order.

## Transforming every group

[`map-groups`]({{ '/functions/grouping/map-groups/' | relative_url }}) evaluates an expression once against each group's complete value collection. It preserves the grouping structure, keys, and group order, and its expression must return a collection.

For example, the grouped orders can be projected to their amounts:

{% raw %}
```expressif
#{
  ("BE" => {
    {country := "BE", customer := "Alice", amount := 120},
    {country := "BE", customer := "Bob", amount := 30},
    {country := "BE", customer := "Alice", amount := 80}
  }),
  ("FR" => {
    {country := "FR", customer := "Claire", amount := 90}
  })
}
| map-groups(map(.amount))
→ #{("BE" => {120, 30, 80}), ("FR" => {90})}
```
{% endraw %}

The expression `map(.amount)` receives one group's array of orders at a time. The result remains a grouping because each key still maps to a collection.

Use `map-groups` for transformations such as filtering values inside every group, projecting fields, or otherwise reshaping each bucket while retaining the ability to apply more grouping-aware operations afterward.

## Aggregating every group

[`summarize`]({{ '/functions/grouping/summarize/' | relative_url }}) also evaluates an expression once against each group's complete value collection, but its purpose is different: it replaces each collection with one summary value and returns a dictionary.

Starting with the projected amounts:

```expressif
#{("BE" => {120, 30, 80}), ("FR" => {90})}
| summarize(sum)
→ !{("BE" => 230), ("FR" => 90)}
```

The `BE` group is reduced to `230` and the `FR` group to `90`. The result uses dictionary syntax because each key now maps to a summary value rather than to a group collection.

### Choosing between `map-groups` and `summarize`

Both functions visit one complete group at a time, but they produce different structures:

| Goal | Function | Result at each key | Result type |
|:-----|:---------|:-------------------|:------------|
| Keep working with groups | `map-groups` | A collection | Grouping |
| Finish with one value per key | `summarize` | Any summary value | Dictionary |

Use `map-groups` when another grouping-aware step must follow. Use `summarize` when the desired output is one total, count, average, record, or other final result for each key.

For example, filtering each country's orders before a later total is a `map-groups` task:

```expressif
#{("BE" => {120, 30, 80}), ("FR" => {90})}
| map-groups(filter(greater-than(50)))
→ #{("BE" => {120, 80}), ("FR" => {90})}
```

Calculating those totals is a `summarize` task:

```expressif
#{("BE" => {120, 80}), ("FR" => {90})}
| summarize(sum)
→ !{("BE" => 200), ("FR" => 90)}
```

## Filtering whole groups

[`filter-groups`]({{ '/functions/grouping/filter-groups/' | relative_url }}) evaluates a predicate against each complete group. It keeps or removes the group as a whole and preserves the grouping type. This gives it the same role as a SQL `HAVING` clause.

For example, keep only countries whose order total is greater than `100`:

```expressif
#{("BE" => {120, 30, 80}), ("FR" => {90})}
| filter-groups($value | sum | greater-than(100))
→ #{("BE" => {120, 30, 80})}
```

The predicate receives a group rather than an individual amount. `$value` selects the complete collection `{120, 30, 80}` or `{90}`, then `sum` calculates the group-level total used by the predicate.

Use ordinary `filter` inside `map-groups` to remove values *within* every group. Use `filter-groups` to remove entire groups based on their key, size, total, or another group-level condition.

## Reusing array operations

A grouping can be consumed by applicable array functions because its array view is the ordered collection of groups. Those functions act on the groups, not directly on every value inside every group.

Using the projected grouping:

```expressif
#{("BE" => {120, 30, 80}), ("FR" => {90})}
```

`cardinality` counts groups:

```expressif
#{("BE" => {120, 30, 80}), ("FR" => {90})}
| cardinality
→ 2
```

`first` selects the first group, after which `$key` can read its key:

```expressif
#{("BE" => {120, 30, 80}), ("FR" => {90})}
| first
| $key
→ "BE"
```

`map` visits the groups. The current element is therefore a group with `$key` and `$value`, not an individual order:

```expressif
#{("BE" => {120, 30, 80}), ("FR" => {90})}
| map($key)
→ {"BE", "FR"}
```

A group itself exposes its grouped values as an array. This makes `map(cardinality)` count the values in each visited group:

```expressif
#{("BE" => {120, 30, 80}), ("FR" => {90})}
| map(cardinality)
→ {3, 1}
```

This fallback does not make ordinary array functions grouping-aware. For example, ordinary `filter` returns an array of selected groups:

```expressif
#{("BE" => {120, 30, 80}), ("FR" => {90})}
| filter($value | cardinality | greater-than(1))
→ {("BE" => {120, 30, 80})}
```

Use the array fallback when the groups themselves are naturally the sequence to count, select, or project. Use `map-groups`, `summarize`, and `filter-groups` when keys and grouping structure must retain their special meaning.

## Putting the workflow together

The complete pipeline groups the orders by country, projects each group to amounts, keeps groups whose total is greater than `100`, and finally calculates one total per remaining country:

{% raw %}
```expressif
{
  {country := "BE", customer := "Alice", amount := 120},
  {country := "FR", customer := "Claire", amount := 90},
  {country := "BE", customer := "Bob", amount := 30},
  {country := "BE", customer := "Alice", amount := 80}
}
| group-by(.country)
| map-groups(map(.amount))
| filter-groups($value | sum | greater-than(100))
| summarize(sum)
→ !{("BE" => 230)}
```
{% endraw %}

The value changes shape deliberately at each stage:

```text
array of orders
      ↓ group-by
grouping of country → orders
      ↓ map-groups
grouping of country → amounts
      ↓ filter-groups
smaller grouping of country → amounts
      ↓ summarize
dictionary of country → total
```
