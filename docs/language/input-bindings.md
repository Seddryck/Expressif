---
title: Input bindings
parent: Expressif language
nav_order: 6
description: Bind an expression's input to a name that remains available throughout its body.
---

An input binding gives a nested expression stable access to the value supplied to it.

Use `^.` and `^^.` for short, local navigation between expression scopes. When a
deeply nested expression deliberately depends on outer data, prefer a named input
binding: `value | name :> body`, referenced as `@name`. Unlike a caret reference,
the name remains stable across nested `map`, function-argument, and grouping
boundaries. Carets navigate expression scopes; they do not navigate to a value's
data parent.

For example, bind an outer record when a calculation uses more than one of its
fields inside a nested transformation:

```expressif
{taxRate := 0.20, prices := {100, 200, 50}}
| source :> .prices
| map(
    multiply(
        @source | .taxRate | add(1) | add(@source | .prices | cardinality)
    )
)
```

The result is `{420.00, 840.00, 210.00}`. `@source` remains the original record
regardless of the nested `map`, `multiply`, `add`, or grouping boundaries.

```expressif
10 | apply(@_ | input :> @input | add(5) | multiply(@input))
```

This returns `150`: the pipeline reaches `15`, while `@input` remains `10`.
The form `@_ | input:> body` is also accepted. A binding requires a preceding pipeline input; `@_` supplies the input of the surrounding call. The `:>` operator must be contiguous.

The name binds the whole input, including scalars, records, arrays, tuples, pairs,
groups, vectors, and null. It does not imply a type or destructure the input.
Names start with an ASCII letter and contain only ASCII letters and digits.
References use `@name`; bare names remain function calls. For example,
`apply(@_ | add :> add(@add))` binds the name `add` without hiding the function `add`.

Names are case-sensitive. An inner binding can shadow an outer binding or a
context variable, including with null. Other outer bindings remain available.
Bindings disappear when their invocation returns or throws; context variables
are never modified. Repeated and concurrent invocations keep separate bindings.

The body includes all pipeline stages up to the enclosing argument boundary.
Inside an explicitly bound body, `$n` and the start of `.field` paths read its
bound input. Ordinary functions still consume the flowing pipeline value.
Invoking a nested expression establishes a new scope; pipeline stages and
grouping parentheses do not. `^$n` reads the current expression input and
`^^$n` reads the enclosing expression input. Each additional caret moves out
one further scope. A missing scope, non-positional input, or unavailable tuple
position returns null, without searching another scope.

Explicit bindings use the same invocation boundary as the implicit binding
already supplied by functions such as `apply`, `map`, and `adjacent`; they do
not add a second scope at that boundary. Existing expressions without `:>`
retain their existing behavior, including callable shorthands.

## Anonymous binding

The anonymous form is `value | :> body`: it establishes the binding behavior but
does not declare a name or make an `@name` reference available. Use `@_ | :> body`
when the surrounding call's input is the value and a name would add no information:

```expressif
adjacent(@_ | :> $1 | subtract($0) | multiply($1))
```

For the pair `T(100, 105)`, this produces `525`. The input remains available
through `$0` and `$1` after subtraction changes the pipeline value to `5`.
No wildcard or synthetic name is required. Nested anonymous bodies establish
new scopes while retaining access to outer named bindings.

For records, `apply(@_ | :> .first | upper | .last)` returns `last` from the bound
record. A chained path such as `.address.city` starts at that record and then
traverses its fields normally. Use explicit functions such as `field(last)` or
`tuple-at(1)` when you intend to select from the flowing intermediate value.

## Positional destructuring

A parenthesized list binds names to positional components:

```expressif
adjacent(@_ | (previous, current) :> @current | subtract(@previous))
V(2, 3, 4) | apply(@_ | (x, y, z) :> @x | multiply(@y) | add(@z))
```

Tuples and vectors expose their components from left to right. Pairs expose
`(key, value)`. Groups expose `(key, values)`, where `values` is the whole group
value collection; the number of items in that collection does not affect the
group's arity of two.

The number of names must exactly match the input's arity. Null, scalars, arrays,
and records cannot be destructured by this positional form. An unsupported type
or arity mismatch raises an argument error. Individual components may be null.

Lists require at least two distinct names. Empty lists, single-name lists,
trailing commas, nested patterns, and rest patterns are not supported. Duplicate
names are rejected during binding with the duplicate's source offset. Commas
inside the list do not terminate the function argument. To bind a whole value,
including a one-element tuple, use an unparenthesized name instead.
