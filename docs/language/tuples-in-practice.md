---
title: Tuples in practice
parent: Expressif language
nav_order: 11
description: Follow a concrete tuple pipeline that compares consecutive measurements and retains significant changes.
---

# Tuples in practice

Tuples are useful when several values belong together during a computation, but do not need the semantics or structure of a record.

A common example is comparing consecutive measurements.

Suppose the following values represent monthly electricity consumption:

```expressif
{120, 135, 128, 150, 162}
```

We want to:

1. compare each value with the previous one;
2. calculate the difference;
3. keep only significant changes;
4. return the current value together with its change.

This can be expressed as:

```expressif
{120, 135, 128, 150, 162}
| pairwise
|> extend($1 | subtract($0))
| filter(tuple-at(2) | absolute | greater-than(10))
|> pick(1, 2)
```

The result is:

```expressif
{
  T(135, 15),
  T(150, 22),
  T(162, 12)
}
```

## Creating pairs

The first step uses `pairwise`:

```expressif
{120, 135, 128, 150, 162}
| pairwise
```

It groups consecutive values into tuples:

```expressif
{
  T(120, 135),
  T(135, 128),
  T(128, 150),
  T(150, 162)
}
```

Each tuple now represents two consecutive measurements:

```text
(previous, current)
```

The tuple is useful here because the two values only need to stay associated while the comparison is performed.

## Extending the tuples

Next, the pipeline calculates the difference between the two values and appends it to each tuple:

```expressif
|> extend($1 | subtract($0))
```

Within each tuple, `$0` refers to the first value and `$1` to the second.

For example:

```expressif
T(120, 135)
```

evaluates:

```expressif
$1 | subtract($0)
```

as:

```text
135 - 120 = 15
```

The tuples therefore become:

```expressif
{
  T(120, 135, 15),
  T(135, 128, -7),
  T(128, 150, 22),
  T(150, 162, 12)
}
```

The third element now contains the change between the previous and current measurement.

## Filtering on a tuple element

We only want changes whose absolute value is greater than `10`:

```expressif
| filter(tuple-at(2) | absolute | greater-than(10))
```

`tuple-at(2)` selects the third element of each tuple.

The value is passed through `absolute` before being compared, so both large increases and large decreases are retained.

The remaining tuples are:

```expressif
{
  T(120, 135, 15),
  T(128, 150, 22),
  T(150, 162, 12)
}
```

## Selecting the useful values

The previous measurement is no longer needed. The final step keeps only the current value and its change:

```expressif
|> pick(1, 2)
```

For example:

```expressif
T(120, 135, 15)
```

becomes:

```expressif
T(135, 15)
```

The final result is:

```expressif
{
  T(135, 15),
  T(150, 22),
  T(162, 12)
}
```

## Why tuples?

This example illustrates an important use of tuples in Expressif.

They are especially useful as **temporary structures created during a transformation**.

Here, the data evolves through several shapes:

```text
values
  ↓
pairs
  ↓
(previous, current, change)
  ↓
(current, change)
```

No named fields are required because the meaning of each element is local to the transformation and its position is sufficient.

Records are generally more appropriate when values have persistent domain meaning and benefit from named fields. Tuples are often preferable when values are combined temporarily for comparison, matching, ranking, or intermediate calculations.
