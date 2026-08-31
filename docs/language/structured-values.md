---
title: Structured values
parent: Expressif language
nav_order: 7
description: Learn how arrays, tuples, and records are constructed, accessed, transformed, and aggregated.
---

Expressif works with structured values as naturally as it works with scalar values.

The three main structured forms are:

```mermaid
flowchart TD
    A[Structured values] --> B[Array]
    A --> C[Tuple]
    A --> D[Record]
```

They solve different problems.

## Arrays

An array represents an ordered sequence of peer values. Its elements may all be available already or may become available progressively as the sequence is consumed.

Example:

```expressif
{1, 2, 3}
```

Arrays are appropriate when the number of values can vary and the values represent the same kind of thing. Collection operations can process the sequence element by element; operations that need to compare, reorder, or inspect the complete sequence may collect its elements before returning a result.

Typical operations include:

```expressif
map(...)
filter(...)
adjacent(...)
sum
count
```

depending on the available function catalog.

## Mapping arrays

`map` transforms every element.

```expressif
@orders
| map(.amount)
```

```mermaid
flowchart LR
    A["array&lt;order&gt;"] --> B["map(.amount)"]
    B --> C["array&lt;numeric&gt;"]
```

The expression passed to `map` is evaluated against each element.

## Filtering arrays

`filter` keeps elements for which a predicate evaluates to true.

```expressif
{1, 2, 3, 4}
| filter(greater-than(2))
```

```mermaid
flowchart LR
    A["{1, 2, 3, 4}"] --> B["filter(greater-than(2))"]
    B --> C["{3, 4}"]
```

The `greater-than(2)` predicate is evaluated for each element. Only values for which it returns `#true` are kept, producing `{3, 4}`.

## Aggregating arrays

Accumulators reduce an array to a result.

```expressif
@orders
| map(.amount)
| sum
```

```mermaid
flowchart LR
    A["array&lt;order&gt;"] --> B["map(.amount)"]
    B --> C["array&lt;numeric&gt;"]
    C --> D[sum]
    D --> E[numeric]
```

Other accumulators can produce text, counts, booleans, or structured results depending on their contract.

## Tuples

A tuple represents a fixed, positional value. Its number of elements is known, every element can be accessed by position, and different positions may have different meanings.

Conceptually:

```expressif
T("Alice", 42)
```

Tuple positions are accessed with positional references such as:

```expressif
$0
$1
```

Positions are zero-based, so `$0` refers to the first value and `$1` to the second.

Tuples are useful when a function needs to expose several related values without assigning field names.

For example, an adjacent-window operation can provide two neighboring values as a tuple-like context.

```mermaid
flowchart LR
    A["previous value"] --> C[Tuple context]
    B["current value"] --> C
    C --> D["$0 / $1"]
```

## Records

A record contains named fields.

Conceptually:

```expressif
{name:="Alice", age:=42}
```

Fields can contain scalar or structured values.

```expressif
{
    name:="Alice",
    totals:={10, 20, 30}
}
```

Fields are accessed by name:

```expressif
.name
.totals
```

## Constructing records

Record construction is useful when you want to project one structure into another.

For example:

```expressif
record(
    id:=.id,
    customer:=.name | upper
)
```

Each field value is itself an expression.

Conceptually:

```mermaid
flowchart TD
    A[Input record] --> B[".id"]
    A --> C[".name | upper"]
    B --> D["id field"]
    C --> E["customer field"]
    D --> F[Output record]
    E --> F
```

This makes records an important tool for shaping data, not only storing it.

## Arrays, tuples, and records are different

Use an array when:

- there can be zero, one, or many elements;
- elements are conceptually peers;
- collection functions should operate over them.

Use a tuple when:

- the number of positions is fixed;
- position has meaning;
- names are unnecessary or would add noise.

Use a record when:

- fields have distinct meanings;
- field names are part of the structure;
- data should be self-describing.

The distinction is based on structure, not size. A two-element array is still an array, while a tuple can contain more than two elements.

## Tuples as arrays

When a function expects an array, Expressif can coerce a tuple to an array. The tuple positions are presented as array elements in the same order. Array operations then produce arrays rather than preserving tuple structure:

```expressif
T(10, 20, 30) | filter(greater-than(10))
```

returns:

```expressif
{20, 30}
```

This direction is implicit because every tuple has a finite, ordered sequence of elements.

The reverse direction is explicit. An array does not state what each position means, and its number of elements may only become known after consuming it. Use `to-tuple` to collect those elements and create a positional value:

```expressif
{10, 20, 30} | to-tuple
```

returns:

```expressif
T(10, 20, 30)
```

The conversion preserves element order and values. It does not recursively convert arrays nested inside the source array.

## Nested structured values

Structured values can be nested.

An array of records:

```expressif
{
    {name:="Alice", age:=42},
    {name:="Bob", age:=37}
}
```

A record containing arrays:

```expressif
{
    customer:="Alice",
    orders:={10, 20, 30}
}
```

A tuple can also contain arrays, records, or other tuples where the function contract allows it.

## Structured transformations stay value-oriented

Even when an expression processes many elements, the same mental model applies:

```mermaid
flowchart LR
    A[Structured value] --> B[Structured expression]
    B --> C[Result value]
```

You do not need to think first in terms of loops. Think in terms of transformations over structured values.
