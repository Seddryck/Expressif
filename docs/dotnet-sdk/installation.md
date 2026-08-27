---
layout: docs
title: Install Expressif
parent: .NET SDK
nav_order: 10
description: Add the Expressif NuGet package to a .NET project.
---

Expressif is distributed as the `Expressif` NuGet package and targets .NET 8, .NET 9, and .NET 10.

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
var expression = new Expression("lower");
var result = expression.Evaluate("Nikola Tesla");
```

`result` is `"nikola tesla"`.

Continue with [Evaluate an expression](../evaluate-expression/) to create an expression, supply input values, and configure runtime variables.
