---
title: Arrays in practice
parent: Expressif language
nav_order: 11
description: Follow a concrete array pipeline that filters order amounts, adds VAT, rounds values, and calculates a total.
---

# Arrays in practice

Arrays are useful when several values form a collection and the same operations need to be applied across that collection.

A common example is calculating a total from a series of amounts.

Suppose the following values represent order amounts before tax:

```expressif
{12.50, 4.00, 27.50, 8.00, 19.00}
```

We want to:

1. keep only orders worth at least `10`;
2. add 21% VAT to each remaining amount;
3. round each amount to two decimal places;
4. calculate the total.

This can be expressed as:

```expressif
{12.50, 4.00, 27.50, 8.00, 19.00}
| filter(greater-than-or-equal(10))
|> (multiply(1.21) | round(2))
| sum
```

The result is:

```expressif
71.40
```

## Starting with a collection

The initial value is an array:

```expressif
{12.50, 4.00, 27.50, 8.00, 19.00}
```

An array represents an ordered collection of values.

In this example, every element has the same role: it is an order amount that will go through the same processing pipeline.

## Filtering the array

The first step removes amounts below `10`:

```expressif
| filter(greater-than-or-equal(10))
```

`filter` evaluates the predicate against every element and keeps only those for which it succeeds.

The array becomes:

```expressif
{12.50, 27.50, 19.00}
```

The operation changes which elements belong to the array, but the result is still an array.

## Transforming every element

Next, VAT is added and each resulting amount is rounded to two decimal places:

```expressif
|> (multiply(1.21) | round(2))
```

The `|>` operator applies the complete expression between parentheses to every element of the array.

For each amount, Expressif first multiplies it by `1.21` and then rounds the result:

```text
12.50 → 15.125 → 15.13
27.50 → 33.275 → 33.28
19.00 → 22.99  → 22.99
```

The resulting array is:

```expressif
{15.13, 33.28, 22.99}
```

This illustrates an important pattern with arrays: `|>` can map an entire pipeline over every element, not just a single function call.

## Reducing the array

Finally, the individual amounts are no longer needed. We only need their total:

```expressif
| sum
```

`sum` consumes the array and combines its elements into a single value:

```text
15.13 + 33.28 + 22.99 = 71.40
```

The final result is therefore:

```expressif
71.40
```

## Why arrays?

This example illustrates a common way to work with arrays in Expressif.

The data evolves through several stages:

```text
order amounts
     ↓
filtered amounts
     ↓
transformed amounts
     ↓
total
```

At each stage, the array represents the collection currently being processed.

`filter` changes which elements belong to the collection. The `|>` operator applies a transformation to every element while preserving the array structure. Finally, `sum` reduces the array to a single value.

Arrays are therefore a natural fit when several values need to be filtered, transformed, or combined as part of the same computation.

## Classifying instead of batching

Use `distribute-condition` when both values that satisfy a condition and values that do not satisfy it are needed:

{% raw %}
```expressif
{1, 2, 3, 4, 5} | distribute-condition(is-even)
→ {{2, 4}, {1, 3, 5}}
```
{% endraw %}

The first output array contains matching values and the second contains non-matching values. Their relative input order is preserved.

This differs from `chunk`, which creates consecutive batches based only on position:

{% raw %}
```expressif
{1, 2, 3, 4, 5} | chunk(2)
→ {{1, 2}, {3, 4}, {5}}
```
{% endraw %}

Choose `distribute-condition` for classification and `chunk` for position-based batching.
