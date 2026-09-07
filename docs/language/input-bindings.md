---
title: Input bindings
parent: Expressif language
nav_order: 6
description: Bind an expression's input to a name that remains available throughout its body.
---

An input binding gives a nested expression stable access to the value supplied to it.

```expressif
10 | apply(input :> @input | add(5) | multiply(@input))
```

This returns `150`: the pipeline reaches `15`, while `@input` remains `10`.
The form `input:> body` is also accepted. The `:>` operator must be contiguous.

The name binds the whole input, including scalars, records, arrays, tuples, pairs,
groups, vectors, and null. It does not imply a type or destructure the input.
Names start with an ASCII letter and contain only ASCII letters and digits.
References use `@name`; bare names remain function calls. For example,
`apply(add :> add(@add))` binds the name `add` without hiding the function `add`.

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
