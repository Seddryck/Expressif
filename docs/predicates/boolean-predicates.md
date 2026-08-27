---
layout: docs
title: "Boolean predicates"
parent: "Predicates library"

nav_order: 20
has_children: true
has_toc: false
permalink: /predicates/boolean-predicates/
tags:
  - predicates
  - boolean

generated: true
---

Reference documentation for Expressif predicates in the `boolean` scope.

| Name | Overview |
|:-----|:---------|
| [`and`]({{ '/predicates/boolean/and/' | relative_url }}) | Returns the logical conjunction of the Boolean-converted input and a secondary predicate expression. Null-like and unconvertible values convert to `false`; zero converts to `false`; nonzero numbers and recognized true text convert to `true`. Evaluates the secondary expression only when the converted input is `true`. |
| [`is-false`]({{ '/predicates/boolean/is-false/' | relative_url }}) | Returns `true` if the argument is effectively `false` else return `false`. |
| [`is-false-or-null`]({{ '/predicates/boolean/is-false-or-null/' | relative_url }}) | Returns `true` if the argument is effectively `false` or `null` else return `false`. |
| [`is-identical-to`]({{ '/predicates/boolean/is-identical-to/' | relative_url }}) | Returns `true` if the boolean passed as argument has the same value than the boolean passed as parameter. Returns `false` otherwise. |
| [`is-true`]({{ '/predicates/boolean/is-true/' | relative_url }}) | Returns `true` if the argument is effectively `true` else return `false`. |
| [`is-true-or-null`]({{ '/predicates/boolean/is-true-or-null/' | relative_url }}) | Returns `true` if the argument is effectively `true` or `null` else return `false`. |
| [`not`]({{ '/predicates/boolean/not/' | relative_url }}) | Returns the logical negation of the Boolean-converted input. Null-like and unconvertible values convert to `false`; zero converts to `false`; nonzero numbers and recognized true text convert to `true`. |
| [`or`]({{ '/predicates/boolean/or/' | relative_url }}) | Returns the logical disjunction of the Boolean-converted input and a secondary predicate expression. Null-like and unconvertible values convert to `false`; zero converts to `false`; nonzero numbers and recognized true text convert to `true`. Evaluates the secondary expression only when the converted input is `false`. |
| [`xor`]({{ '/predicates/boolean/xor/' | relative_url }}) | Returns `true` when exactly one of the Boolean-converted input and a secondary predicate expression evaluates to `true`. Null-like and unconvertible values convert to `false`; zero converts to `false`; nonzero numbers and recognized true text convert to `true`. Always evaluates the secondary expression. |
