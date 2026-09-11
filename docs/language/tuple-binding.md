---
layout: default
title: Tuple binding
---

# Tuple binding

`T(input, arg1, arg2) | bind("f")` invokes `input | f(arg1, arg2)` using already-evaluated values. `f~` is the same operation. `~f` expands to `rotate | bind("f")`: the default offset is **1**, positive offsets rotate right, and the last position moves to the front without reversing the remaining arguments.

```expressif
T(120, 135) | subtract~        // -15
T(120, 135) | ~subtract        // 15
T(2, 3, 20) | ~subtract       // 14: 20 - 2 * 3
{20, 3, 2} | reduce(subtract~) // 15
"aaabb" | split-while(starts-with~) // {"aaa", "bb"}
```

Functions, predicates and custom callables share signature-level eligibility. Ordinary value arguments, optional signatures and adapted value variadics are supported. Unevaluated expressions, predicates and transformation arguments require an explicit compatible adapter. Unknown targets, ineligible signatures and invalid invocation arity or values have separate diagnostics. Unknown tuple shape alone does not make a target ineligible.

| Operator | Invocation tuple | Typical form |
|---|---|---|
| adjacent | T(previous, current) | adjacent(~subtract) |
| chunk-while | T(previous, current) | chunk-while(~subtract &#124; less-than(2)) |
| map-over | T(outer input, ...item arguments) | map-over(subtract~, values) |
| map-with | T(outer input, item) | map-with(~subtract, values) |
| reduce | T(accumulator, current item) | reduce(subtract~) |
| split-while | T(current segment, next character) | split-while(starts-with~) |

Directional maps prepare a tuple only at a leading binding operation, including a leading `rotate` or `rotate(1)` followed by `bind`. Parentheses around an open operation are transparent; input-bound expressions introduce their own boundary. A later binding consumes the preceding stage's tuple. `map-over` expands only the supplied item tuple by one level; the outer input and nested tuple items remain intact. `map-with` preserves the whole supplied item.

Following stages receive the target result, while their argument expressions retain the supplied item context. For example, `5 | map-over(subtract~ | add(@_), {10, 11})` yields `{5, 5}`: `@_` is 10 then 11, not the prepared invocation tuple. For `map-with`, `@_` also retains the supplied item, so `5 | map-with(~subtract | add(@_), {10, 11})` yields `{15, 17}`.

## Migrating implicit binding

Legacy `adjacent(subtract)`, `chunk-while(subtract | less-than(2))`, `map-over(subtract, values)` and `map-with(subtract, values)` remain available for compatibility. Only reliance on injected arguments is deprecated now; the surrounding operators and target callables are not. Prefer `~subtract` for adjacent, chunk-while and map-with, and `subtract~` for map-over. Removal planned for v3.0. See [Deprecated usage patterns]({{ '/deprecations/#deprecated-usage-patterns' | relative_url }}) for shared lifecycle rules and supported replacement expressions.

On this development line, `adjacent` and `chunk-while` supply `T(previous, current)` to their operation: `$0` reads the previous element and `$1` the current element of the array supplied to that call. Thus `{1, 2, 5} | adjacent($1 | subtract($0))` and `{1, 2, 5} | adjacent(~subtract)` both return `{1, 3}`. Before migrating from an older runtime, verify its `chunk-while` context; positional replacements depend on that tuple layout. Following stages of a `chunk-while` operation receive the preceding result as pipeline input while retaining the pair as their argument context.

The shared semantic API exposes the resolved callable, signature, source span and input/argument mapping. Automatic replacement is withheld when invocation equivalence cannot be established (for example, an unknown tuple shape or incompatible values). Existing complete reduce and split-while expressions are not deprecated, including reduce's leading `$0` normalization.
