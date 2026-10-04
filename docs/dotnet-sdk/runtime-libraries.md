---
layout: docs
title: Load runtime libraries
parent: .NET SDK
nav_order: 65
description: Register independently released Expressif libraries in an isolated host environment.
---

An `ExpressifEnvironment` is an immutable snapshot of the libraries available to one host. Register a library assembly before creating the binder that will compile expressions against it:

```csharp
using Expressif.Hosting;
using Expressif.Library.SemVer;

var environment = ExpressifEnvironment.Default
    .RegisterLibrary<SemVerLibrary>();

var expression = environment.CreateClosedExpression(
    "#\"1.2.3-rc.1+build.7\":semver | bump-patch");

var result = expression.Evaluate(null); // 1.2.4
```

`RegisterLibrary(Assembly)` is available when the host loads an assembly without a compile-time marker type. Expressif does not download packages or select library versions: the host supplies the exact assembly.

Registration validates the library manifest, core API compatibility, declared library dependencies, embedded callable catalogs, type and literal parsers, coercions, and implementation names. A failure throws `LibraryRegistrationException` and leaves the original environment unchanged.

Each registration returns a new environment. Existing environments and expressions already bound from them keep their previous capabilities, so separate hosts can safely use different library sets or versions. Create textual expressions, textual predications, and typed builders from the registered environment; all of them then use that exact library snapshot. Registration never changes `ExpressifEnvironment.Default` and does not scan ambient assemblies.

## Library assembly contract

A library declares its identity and compatibility at assembly level:

```csharp
using Expressif.Hosting;

[assembly: ExpressifLibrary(
    "Contoso.Expressif.Library",
    "1.2.0",
    "3.0.0",
    "4.0.0")]

[assembly: ExpressifLibraryDependency(
    "Expressif.Library",
    "3.0.0",
    "4.0.0")]
```

The assembly contributes capabilities through the public Core extension contracts:

- functions and predicates marked with their Expressif attributes;
- coercion descriptors;
- type descriptors marked with `ExpressifType`;
- quoted literal parsers implementing `IQuotedLiteralParser`;
- `Expressif.FunctionCatalog.json` and `Expressif.PredicateCatalog.json` embedded resources.

The combined catalog and type registry are available through `environment.Catalog` and `environment.Types` for host introspection.
