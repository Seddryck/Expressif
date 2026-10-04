---
layout: docs
title: Sovereign by design
nav_order: 2.5
permalink: /sovereign-by-design/
description: How Expressif supports independent ownership, operation, implementation, and migration of expression logic.
---

Expressif is sovereign by design: an open, self-hostable, and portable expression language and execution technology that can be implemented and operated independently of a specific cloud, vendor, or execution engine.

"Sovereign by design" describes architectural and governance properties. It is not a certification, legal status, or guarantee that every deployment meets an organization's regulatory requirements. Those requirements still depend on how that organization hosts, governs, and integrates Expressif.

## Control remains with the user

Expressif does not require an Expressif-hosted service, account, license server, or external control plane to evaluate an expression. An organization can run the [command-line interface]({{ '/cli/' | relative_url }}) or embed the [.NET SDK]({{ '/dotnet-sdk/' | relative_url }}) in infrastructure that it selects and operates, including disconnected environments.

The source code is available under the [Apache License 2.0](https://github.com/Seddryck/Expressif/blob/main/LICENSE). That license permits users, subject to its terms, to inspect, build, modify, distribute, and fork the software. Access to an original vendor is therefore not a technical prerequisite for maintaining a deployment or developing a replacement.

This also makes Expressif independent of a particular cloud or SaaS provider. A hosting provider can be useful, but it is a replaceable deployment choice rather than part of the language's execution contract.

## Language artifacts are portable

An expression is language source, not a request to a particular hosted service. Its meaning is defined by the documented [language semantics]({{ '/language/' | relative_url }}), including [argument contexts]({{ '/language/argument-contexts/' | relative_url }}), [structural semantics]({{ '/language/structural-semantics/' | relative_url }}), and [failure behavior]({{ '/language/failure-semantics/' | relative_url }}), rather than by private behavior in a single deployment.

[Portable logical plans]({{ '/language/logical-plan-format/' | relative_url }}) provide a machine-readable interchange form after parsing and planning. They record canonical operators, semantic types, argument contexts, evaluation rules, and structural semantics without embedding CLR types, assemblies, delegates, or backend objects. A plan can therefore be stored, inspected, transported, or consumed by another compatible execution environment without retaining the process that produced it.

Portability has limits that are made explicit. A consumer must support the plan format, compatible catalog, operators, types, and extensions used by a plan. Keeping those requirements visible is preferable to hiding an execution-engine dependency in an opaque artifact.

## Semantics can have independent implementations

The .NET implementation and the project's separate [Python implementation work](https://github.com/Seddryck/Expressif/tree/feat/python/python) demonstrate that Expressif is not intrinsically coupled to the CLR or to one implementation architecture. Implementations need not share source code; they need to share the observable language contract.

The language guide and reference catalog make that contract readable independently of the .NET runtime. The [logical-plan JSON schema](https://github.com/Seddryck/Expressif/blob/feat/v3.0/docs/_data/logical-plan.schema.json) and the [conformance schema](https://github.com/Seddryck/Expressif/blob/feat/v3.0/conformance/conformance.schema.json) make important parts of it machine-readable. Together, these materials let another organization design an Expressif-compatible parser, planner, or evaluator without depending on private APIs or permission from a particular vendor.

The existence of multiple implementations does not by itself prove identical behavior. Compatibility is established feature by feature through conformance.

## Conformance makes compatibility testable

The shared [YAML conformance suite](https://github.com/Seddryck/Expressif/tree/feat/v3.0/conformance) describes language-independent inputs, parameters, contexts, and expected results for functions and predicates. The same cases can be executed against different implementations, so compatibility is based on observable results rather than a claim that one codebase is the only specification.

Published manifests identify the suite version, source revision, included files, and case counts. Implementations can publish results against that material, and users can obtain released conformance assets from the [download page]({{ '/download/' | relative_url }}). This provides a repeatable basis for finding semantic gaps, comparing implementations, and maintaining compatibility as the language evolves.

Conformance is evidence with a defined scope, not a blanket certification. An implementation's result applies to the suite version and features that it actually ran.

## Interoperability is compatible with sovereignty

Sovereignty does not mean isolation or technological autarky. An organization can use cloud platforms, external engines, and third-party tools while remaining sovereign when it retains meaningful control of its expressions and plans, can operate them in an environment of its choice, and can replace an integration without rewriting the underlying intent.

Expressif's logical plans create a stable boundary for adapters to other planners and execution engines. Open standards can extend that boundary. For example, [Substrait](https://substrait.io/) defines a cross-language format for relational compute operations; where Expressif and Substrait semantics overlap, an integration can translate supported operations instead of binding expression logic directly to one engine. This is an interoperability path, not a claim that every Expressif plan currently has a complete or lossless Substrait mapping.

The result is practical independence: users can keep their language artifacts, verify compatible behavior, operate on infrastructure they control, and introduce or replace implementations and execution engines over time.
