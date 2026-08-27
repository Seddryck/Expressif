# Expressif

![Expressif](https://raw.githubusercontent.com/Seddryck/Expressif/main/docs/assets/images/expressif-logo-with-name.png)

## About

**Social media:** [![website](https://img.shields.io/badge/website-seddryck.github.io/Expressif-fe762d.svg)](https://seddryck.github.io/Expressif)
[![twitter badge](https://img.shields.io/badge/twitter%20Expressif-@Seddryck-blue.svg?style=flat&logo=twitter)](https://twitter.com/Seddryck)

**Releases:** [![nuget](https://img.shields.io/nuget/v/Expressif.svg)](https://www.nuget.org/packages/Expressif/) [![GitHub Release Date](https://img.shields.io/github/release-date/seddryck/Expressif.svg)](https://github.com/Seddryck/Expressif/releases/latest) ![conformance manifest badge](https://img.shields.io/github/v/tag/Seddryck/Expressif?filter=conformance-*&label=manifest) [![licence badge](https://img.shields.io/badge/License-Apache%202.0-yellow.svg)](https://github.com/Seddryck/Expressif/blob/master/LICENSE)

**Dev. activity:** [![GitHub last commit](https://img.shields.io/github/last-commit/Seddryck/Expressif.svg)](https://github.com/Seddryck/Expressif/commits)
![Still maintained](https://img.shields.io/maintenance/yes/2026.svg)
![GitHub commit activity](https://img.shields.io/github/commit-activity/y/Seddryck/Expressif)

**Continuous integration builds:**
[![CI](https://github.com/Seddryck/Expressif/actions/workflows/ci.yml/badge.svg?branch=next-major)](https://github.com/Seddryck/Expressif/actions/workflows/ci.yml?query=branch%3Anext-major)
[![CodeFactor](https://www.codefactor.io/repository/github/seddryck/expressif/badge)](https://www.codefactor.io/repository/github/seddryck/expressif)
[![codecov](https://codecov.io/github/Seddryck/Expressif/branch/main/graph/badge.svg?token=9ZSJ6N0X9E)](https://codecov.io/github/Seddryck/Expressif)
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FSeddryck%2FExpressif.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2FSeddryck%2FExpressif?ref=badge_shield)

**Status:** [![stars badge](https://img.shields.io/github/stars/Seddryck/Expressif.svg)](https://github.com/Seddryck/Expressif/stargazers)
[![Bugs badge](https://img.shields.io/github/issues/Seddryck/Expressif/bug.svg?color=red&label=Bugs)](https://github.com/Seddryck/Expressif/issues?utf8=%E2%9C%93&q=is:issue+is:open+label:bug+)
[![Features badge](https://img.shields.io/github/issues/seddryck/Expressif/new-feature.svg?color=purple&label=Feature%20requests)](https://github.com/Seddryck/Expressif/issues?utf8=%E2%9C%93&q=is:issue+is:open+label:new-feature+)
[![Top language](https://img.shields.io/github/languages/top/seddryck/Expressif.svg)](https://github.com/Seddryck/Expressif/search?l=C%23)

Expressif is a language for defining data transformations, validations, computations, and aggregations as expressions. It was created for [nbi.io](https://www.nbi.io) and has grown into a portable language and function library for working with scalar and structured data.

Expressions are designed to be:

- **Readable:** values flow from left to right through explicit transformations.
- **Predictable:** operations favor safe results, such as `#null`, over avoidable exceptions.
- **Composable:** small expressions combine into pipelines for filtering, mapping, and aggregation.
- **Portable:** the language is designed to retain its meaning across runtime ecosystems.

[Read the documentation](https://seddryck.github.io/Expressif/) · [Learn the language](https://seddryck.github.io/Expressif/language/) · [Browse the reference](https://seddryck.github.io/Expressif/functions/) · [View releases](https://github.com/Seddryck/Expressif/releases)

## The language

An Expressif expression receives a value, applies a transformation, and produces a result. The pipe operator (`|`) passes the output of each operation to the next one.

```expressif
"  Alice  " | trim | upper
```

The same model scales to structured data and collections:

```expressif
@orders
| filter(.active)
| map(.amount)
| sum
```

Predicates are expressions that produce Boolean values. Predicate combinators apply multiple conditions to the same input:

```expressif
greater-than(0) |AND less-than(100)
```

See the language guide for [expressions](https://seddryck.github.io/Expressif/language/expressions/), [values and types](https://seddryck.github.io/Expressif/language/values-and-types/), [references](https://seddryck.github.io/Expressif/language/references/), [structured values](https://seddryck.github.io/Expressif/language/structured-values/), and [predicates](https://seddryck.github.io/Expressif/language/predicates/).

## Use Expressif

### .NET SDK

The `Expressif` NuGet package targets .NET 8, .NET 9, and .NET 10.

```bash
dotnet add package Expressif
```

Create an expression and evaluate an input value:

```csharp
using Expressif;

var expression = Expression.Create("trim | upper");
var result = expression.Evaluate("  Alice  ");
```

The .NET SDK also provides APIs for predications, typed builders, runtime context, and serialization. Continue with the [.NET SDK guide](https://seddryck.github.io/Expressif/dotnet-sdk/).

### Command-line interface

Install the CLI as a .NET global tool:

```bash
dotnet tool install --global Expressif-cli
expressif version
```

The CLI can evaluate expressions, process input data, validate source text, inspect parsing and binding, and display function help.

```bash
expressif evaluate '"  Alice  " | trim | upper'
```

Windows installers and portable archives for Windows and Linux are also available from [GitHub Releases](https://github.com/Seddryck/Expressif/releases). See the [CLI guide](https://seddryck.github.io/Expressif/cli/) for commands, input sources, diagnostics, and automation guidance.

## Documentation

The documentation site is the authoritative guide and reference:

- [Getting started](https://seddryck.github.io/Expressif/getting-started/)
- [Language guide](https://seddryck.github.io/Expressif/language/)
- [Function reference](https://seddryck.github.io/Expressif/functions/)
- [Predicate reference](https://seddryck.github.io/Expressif/predicates/)
- [Accumulator reference](https://seddryck.github.io/Expressif/accumulators/)
- [.NET SDK](https://seddryck.github.io/Expressif/dotnet-sdk/)
- [Command-line interface](https://seddryck.github.io/Expressif/cli/)
- [Tooling and editor support](https://seddryck.github.io/Expressif/tooling/)

Function, predicate, and accumulator details live in these references rather than being duplicated in this README.

## Tooling

Expressif provides syntax highlighting for Visual Studio Code, TextMate consumers, Notepad++, and Rouge. Language-aware diagnostics and completion are available through the [Expressif Language Server](https://github.com/Seddryck/Expressif.LanguageServer).

Download the editor assets that match your Expressif version from [GitHub Releases](https://github.com/Seddryck/Expressif/releases), then follow the [tooling documentation](https://seddryck.github.io/Expressif/tooling/) for installation and capabilities.

## Contributing

Issues, feature ideas, and pull requests are welcome. Read [CONTRIBUTING.md](CONTRIBUTING.md) before making a change, use [GitHub Discussions](https://github.com/Seddryck/Expressif/discussions) for broader questions, or report a problem through [GitHub Issues](https://github.com/Seddryck/Expressif/issues).

## License

Expressif is licensed under the [Apache License 2.0](LICENSE).
