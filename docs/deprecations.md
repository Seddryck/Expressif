---
layout: docs
title: Deprecations and sunsets
nav_order: 8.25
permalink: /deprecations/
description: Deprecated Expressif language callables, their replacements, and planned removal versions.
---

Deprecation means that a callable remains available for compatibility but should not be used in new expressions. A sunset gives the Expressif version in which removal is planned.

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

This page is generated from the function, predicate, and accumulator catalogs. It does not describe the lifecycle of the public .NET API.
