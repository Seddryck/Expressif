---
layout: docs
title: Install Expressif
parent: .NET SDK
nav_order: 10
description: Add the Expressif NuGet package to a .NET project.
---

Expressif targets .NET 8, .NET 9, and .NET 10. Most applications should install the `Expressif` umbrella package, which brings in both the language runtime and the official function library.

## Install with the .NET CLI

Run this command from the project directory:

```bash
dotnet add package Expressif
```

To select a specific version, add `--version`:

```bash
dotnet add package Expressif --version <version>
```

## Install with Visual Studio

In the Package Manager Console, run:

```powershell
Install-Package Expressif
```

You can also open **Manage NuGet Packages**, search for `Expressif`, and install the package into the required project.

## Use the namespace

Add the namespace in a C# file:

```csharp
using Expressif;
```

Then evaluate a small expression:

```csharp
var expression = Expression.Create("lower");
var result = expression.Evaluate("Nikola Tesla");
```

`result` is `"nikola tesla"`.

## Choose a package

| Package | Contents | Intended use |
|:--|:--|:--|
| `Expressif` | References `Expressif.Core` and `Expressif.Library`. | The recommended package for applications using the standard Expressif language. |
| `Expressif.Core` | Public contracts, parsing and binding infrastructure, values, evaluation runtime, and extension points. | Hosts that provide a custom or third-party vocabulary without the official built-ins. |
| `Expressif.Library` | Official functions, predicates, accumulators, constants, catalogs, and default expression composition. References `Expressif.Core`. | Consumers that explicitly want the official vocabulary; Core is installed transitively. |

The package graph has no cycle:

```text
             Expressif
            /         \
           v           v
Expressif.Core <- Expressif.Library
```

For a custom host, install only Core:

```bash
dotnet add package Expressif.Core
```

The three packages are built, versioned, and released together. Their major, minor, patch, and prerelease versions must match; mixing versions is unsupported. The umbrella and Library packages declare dependencies from the same build version so normal NuGet resolution selects a compatible set.

Continue with [Evaluate an expression](../evaluate-expression/) to create an expression, supply input values, and configure runtime variables.
