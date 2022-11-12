# Expressif
Expressif is the variable substitution syntax, initially designed for [NBi.io](https://www.nbi.io).

Expressif allows you to define variables and transformation of these variables (functions), in plain text, which can then be interpreted by the engine. The syntax for the definition of the expression transforming the variable is similar to:

```
@myVariable | text-to-lower | text-to-pad-right(@myCount, *)
```

![Logo](https://raw.githubusercontent.com/Seddryck/Expressif/main/misc/icon/expressif-icon-256.png)

[About][] | [Quickstart][] | [Installing][] | [Functions][]

[About]: #about (About)
[Quickstart]: #quickstart (Quickstart)
[Installing]: #installing (Installing)
[Functions]: #functions (Functions)

## About

**Social media:**
[![twitter badge](https://img.shields.io/badge/twitter%20Expressif-@Seddryck-blue.svg?style=flat&logo=twitter)](https://twitter.com/Seddryck)

**Releases:** [![nuget](https://img.shields.io/nuget/v/Expressif.svg)](https://www.nuget.org/packages/Expressif/)<!-- [![GitHub Release Date](https://img.shields.io/github/release-date/seddryck/Expressif.svg)](https://github.com/Seddryck/Expressif/releases/latest) --> [![licence badge](https://img.shields.io/badge/License-Apache%202.0-yellow.svg)](https://github.com/Seddryck/Expressif/blob/master/LICENSE)
<!-- [![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FSeddryck%2FExpressif.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2FSeddryck%2FExpressif?ref=badge_shield) -->

**Dev. activity:** [![GitHub last commit](https://img.shields.io/github/last-commit/Seddryck/Expressif.svg)](https://github.com/Seddryck/Expressif/commits)
![Still maintained](https://img.shields.io/maintenance/yes/2022.svg)
![GitHub commit activity](https://img.shields.io/github/commit-activity/y/Seddryck/Expressif)

**Continuous integration builds:** [![Build status](https://ci.appveyor.com/api/projects/status/7btqredpvl803ri5?svg=true)](https://ci.appveyor.com/project/Seddryck/Expressif/)
[![Tests](https://img.shields.io/appveyor/tests/seddryck/Expressif.svg)](https://ci.appveyor.com/project/Seddryck/Expressif/build/tests)

**Status:** [![stars badge](https://img.shields.io/github/stars/Seddryck/Expressif.svg)](https://github.com/Seddryck/Expressif/stargazers)
[![Bugs badge](https://img.shields.io/github/issues/Seddryck/Expressif/bug.svg?color=red&label=Bugs)](https://github.com/Seddryck/Expressif/issues?utf8=%E2%9C%93&q=is:issue+is:open+label:bug+)
[![Features badge](https://img.shields.io/github/issues/seddryck/Expressif/new-feature.svg?color=purple&label=Feature%20requests)](https://github.com/Seddryck/Expressif/issues?utf8=%E2%9C%93&q=is:issue+is:open+label:new-feature+)
[![Top language](https://img.shields.io/github/languages/top/seddryck/Expressif.svg)](https://github.com/Seddryck/Expressif/search?l=C%23)

## Quickstart

```csharp
var factory = new ExpressionFactory();
var expression = factory.Instantiate("text-to-lower");
var result = expression.Execute("Nikola Tesla")
Debug.WriteLine(result); // returns "nikola tesla"
```

Some functions required arguments, you can specify them between the brackets after the function name. Note that literal textual values don't required quotes surronding the values.

```csharp
var factory = new ExpressionFactory();
var expression = factory.Instantiate("text-to-remove-chars(a)");
var result = expression.Execute("Nikola Tesla")
Debug.WriteLine(result); // returns "Nikol Tesl"
```

You can chain the functions to apply to the initial value by using the operator pipe (`|`). The functions are executed from left to right.

```csharp
var factory = new ExpressionFactory();
var expression = factory.Instantiate("text-to-lower | text-to-remove-chars(a)");
var result = expression.Execute("Nikola Tesla")
Debug.WriteLine(result); // returns "nikol tesl"
```

It's possible to use variables as function parameters. the name of the variables must always start by an arobas (`@`)

```csharp
var context = new Context();
context.Variables.Add("myChar", 'k');

var factory = new ExpressionFactory(context);
var expression = factory.Instantiate("text-to-lower | text-to-remove-chars(@myChar)");
var result = expression.Execute("Nikola Tesla")
Debug.WriteLine(result); // returns "niola tesla"
```

In addition to the variables that must be scalar values (text, numeric, dateTime ...), you can also add a property-object to the context. An property-object can be a pure C# object, a Dictionnary, a List or a DataRow. You can access the properties of the property-object based on the property's name with the syntax `[property-name]`

```csharp
var context = new Context();
context.PropertyObject.Set(new {CharToBeRemoved = 't'});

var factory = new ExpressionFactory(context);
var expression = factory.Instantiate("text-to-lower | text-to-remove-chars([CharToBeRemoved])");
var result = expression.Execute("Nikola Tesla")
Debug.WriteLine(result); // returns "nikola esla"
```

or based on its position with the syntax `#index` (where index is positive number).

```csharp
var context = new Context();
context.PropertyObject.Set(new List() {'e', 's'});

var factory = new ExpressionFactory(context);
var expression = factory.Instantiate("text-to-lower | text-to-remove-chars(#1)");
var result = expression.Execute(""Nikola Tesla"")
Debug.WriteLine(result); // returns "nikola tela"
```

It's also possible to use a function's result as the value of a parameter for another function. To achieve this the function as a parameter must be surrounded by curly braces `{...}`

```csharp
var context = new Context();
context.Variable.Add("myVar", 6)
context.PropertyObject.Set(new List() {15, 8, 3});

var factory = new ExpressionFactory(context);
var expression = factory.Instantiate("text-to-lower | text-to-skip-last-chars( {@myVar | numeric-to-subtract(#2) }));
var result = expression.Execute(""Nikola Tesla"")
Debug.WriteLine(result); // sub-function returns 6-3 = 3 and the main function returns "nikola te"
```

## Installing

Install in the usual .NET fashion:

```sh
Install-Package Expressif
```

## Functions

TBC
