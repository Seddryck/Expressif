---
layout: docs
title: Deprecations and sunsets
nav_order: 8.25
permalink: /deprecations/
description: Deprecated Expressif callables and usage patterns, migration expressions, and planned removal versions.
---

Deprecation means that a callable or usage pattern remains available for compatibility but should not be used in new expressions. Deprecation is effective now; a sunset is the separate version in which removal is planned.

## Deprecated callables

{% assign deprecated_functions = site.data.function | where: "IsPublic", true | where: "Deprecated", true %}
{% assign deprecated_predicates = site.data.predicate | where: "IsPublic", true | where: "Deprecated", true %}
{% assign deprecated_accumulators = site.data.accumulator | where: "IsPublic", true | where: "Deprecated", true %}
{% assign deprecated_callables = deprecated_functions | concat: deprecated_predicates | concat: deprecated_accumulators %}

{% if deprecated_callables.size > 0 %}
<table>
  <thead>
    <tr>
      <th>Kind</th>
      <th>Callable</th>
      <th>Use instead</th>
      <th>Sunset</th>
    </tr>
  </thead>
  <tbody>
{% include language-deprecation-rows.html catalog=site.data.function kind="Function" kind_plural="functions" %}
{% include language-deprecation-rows.html catalog=site.data.predicate kind="Predicate" kind_plural="predicates" %}
{% include language-deprecation-rows.html catalog=site.data.accumulator kind="Accumulator" kind_plural="accumulators" %}
  </tbody>
</table>
{% else %}
There are currently no deprecated public language callables.
{% endif %}

## Deprecated usage patterns

These forms implicitly supply arguments to a nested callable. Replace that usage with explicit tuple binding; the surrounding operators and target callable names remain supported. Removal planned for v3.0.

Shorthand replacements below are supported on the current development line. For an older installed release, check its tuple-binding support before using tilde syntax. The explicit-expression alternatives apply where their stated context and signature conditions hold. Automatic migration requires a resolved eligible signature and compatible invocation values; unknown tuple shapes or incompatible values withhold automatic replacement.

{% assign deprecated_usages = site.data.usage-lifecycle | where: "Active", true %}
{% if deprecated_usages.size > 0 %}
{% for rule in deprecated_usages %}
{% include language-usage-deprecation.html rule=rule %}
{% endfor %}
{% else %}
There are currently no deprecated usage patterns.
{% endif %}

Existing complete `reduce` and `split-while` expressions are not deprecated solely because those operators support shorthand binding.

Callable entries come from the function, predicate, and accumulator catalogs. Usage entries come from the same structured lifecycle rules embedded in the runtime and exposed by semantic diagnostics for language-server migration support. `DeprecatedSince` records the actual introducing release when published; a pending release version does not make an active deprecation pending. This page does not describe the lifecycle of the public .NET API.
