# Expressif
Expressif is the variable substitution syntax, initially designed for [NBi.io](https://www.nbi.io).

Expressif allows you to define variables and transformation of these variables (functions), in plain text, which can then be interpreted by the engine. The syntax for the definition of the expression transforming the variable is similar to:

```
@myVariable | text-to-lower | text-to-pad-right(@myCount, *)
```

![Logo](https://raw.githubusercontent.com/Seddryck/Expressif/main/misc/icon/expressif-icon-256.png)

[About][] | [Quickstart][] | [Installing][] | [Functions and predicates][]

[About]: #about (About)
[Quickstart]: #quickstart (Quickstart)
[Installing]: #installing (Installing)
[Functions and predicates]: #supported (Functions and predicates)

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
var expression = new Expression("text-to-lower");
var result = expression.Evaluate("Nikola Tesla");
Assert.That(result, Is.EqualTo("nikola tesla"));
```

Some functions required arguments, you can specify them between the brackets after the function name. Note that literal textual values don't required quotes surronding the values.

```csharp
var expression = new Expression("text-to-remove-chars(a)");
var result = expression.Evaluate("Nikola Tesla");
Assert.That(result, Is.EqualTo("Nikol Tesl"));
```

You can chain the functions to apply to the initial value by using the operator pipe (`|`). The functions are executed from left to right.

```csharp
var expression = new Expression("text-to-lower | text-to-remove-chars(a)");
var result = expression.Evaluate("Nikola Tesla");
Assert.That(result, Is.EqualTo("nikol tesl"));
```
It's possible to use variables as function parameters. the name of the variables must always start by an arobas (`@`)

```csharp
var context = new Context();
context.Variables.Add<char>("myChar", 'k');

var expression = new Expression("text-to-lower | text-to-remove-chars(@myChar)", context);
var result = expression.Evaluate("Nikola Tesla");
Assert.That(result, Is.EqualTo("niola tesla"));
```

In addition to the variables that must be scalar values (text, numeric, dateTime ...), you can also add a property-object to the context. An property-object can be a pure C# object, a Dictionnary, a List or a DataRow. You can access the properties of the property-object based on the property's name with the syntax `[property-name]`

```csharp
var context = new Context();
context.CurrentObject.Set(new { CharToBeRemoved = 't' });

var expression = new Expression("text-to-lower | text-to-remove-chars([CharToBeRemoved])", context);
var result = expression.Evaluate("Nikola Tesla");
Assert.That(result, Is.EqualTo("nikola esla"));
```

or based on its position with the syntax `#index` (where index is positive number).

```csharp
var context = new Context();
context.CurrentObject.Set(new List<char>() { 'e', 's' });

var expression = new Expression("text-to-lower | text-to-remove-chars(#1)", context);
var result = expression.Evaluate("Nikola Tesla");
Assert.That(result, Is.EqualTo("nikola tela"));
```

It's also possible to use a function's result as the value of a parameter for another function. To achieve this the function as a parameter must be surrounded by curly braces `{...}`

```csharp
var context = new Context();
context.Variables.Add<int>("myVar", 6);
context.CurrentObject.Set(new List<int>() { 15, 8, 3 });

var expression = new Expression("text-to-lower | text-to-skip-last-chars( {@myVar | numeric-to-subtract(#2) })", context);
var result = expression.Evaluate("Nikola Tesla");
Assert.That(result, Is.EqualTo("nikola te"));
```

## Installing

Install in the usual .NET fashion:

```sh
Install-Package Expressif
```

## Supported functions and predicates

### Functions
<!-- START FUNCTION TABLE -->
|Scope    | Name                       | Aliases                               |
|-------- | -------------------------- | --------------------------------------|
|IO       | creation-datetime          | file-to-creation-datetime             |
|IO       | creation-datetime-utc      | file-to-creation-datetime-utc         |
|IO       | directory                  | path-to-directory                     |
|IO       | extension                  | path-to-extension                     |
|IO       | filename                   | path-to-filename                      |
|IO       | filename-without-extension | path-to-filename-without-extension    |
|IO       | root                       | path-to-root                          |
|IO       | size                       | file-to-size                          |
|IO       | update-datetime            | file-to-update-datetime               |
|IO       | update-datetime-utc        | file-to-update-datetime-utc           |
|Numeric  | add                        | numeric-to-add                        |
|Numeric  | ceiling                    | numeric-to-ceiling                    |
|Numeric  | clip                       | numeric-to-clip                       |
|Numeric  | decrement                  | numeric-to-decrement                  |
|Numeric  | divide                     | numeric-to-divide                     |
|Numeric  | floor                      | numeric-to-floor                      |
|Numeric  | increment                  | numeric-to-increment                  |
|Numeric  | integer                    | numeric-to-integer                    |
|Numeric  | invert                     | numeric-to-invert                     |
|Numeric  | multiply                   | numeric-to-multiply                   |
|Numeric  | null-to-zero               |                                       |
|Numeric  | round                      | numeric-to-round                      |
|Numeric  | subtract                   | numeric-to-subtract                   |
|Special  | any-to-any                 |                                       |
|Special  | neutral                    |                                       |
|Special  | null-to-value              |                                       |
|Special  | value-to-value             |                                       |
|Temporal | age                        | dateTime-to-age                       |
|Temporal | back                       | dateTime-to-back, dateTime-to-subtract|
|Temporal | ceiling-hour               | dateTime-to-ceiling-hour              |
|Temporal | ceiling-minute             | dateTime-to-ceiling-minute            |
|Temporal | cut-off                    | dateTime-to-cut-off, dateTime-to-clip |
|Temporal | datetime-to-date           | dateTime-to-datetime-to-date          |
|Temporal | first-of-month             | dateTime-to-first-of-month            |
|Temporal | first-of-year              | dateTime-to-first-of-year             |
|Temporal | floor-hour                 | dateTime-to-floor-hour                |
|Temporal | floor-minute               | dateTime-to-floor-minute              |
|Temporal | forward                    | dateTime-to-forward, dateTime-to-add  |
|Temporal | invalid-to-date            |                                       |
|Temporal | last-of-month              | dateTime-to-last-of-month             |
|Temporal | last-of-year               | dateTime-to-last-of-year              |
|Temporal | local-to-utc               |                                       |
|Temporal | next-day                   | dateTime-to-next-day                  |
|Temporal | next-month                 | dateTime-to-next-month                |
|Temporal | next-year                  | dateTime-to-next-year                 |
|Temporal | null-to-date               |                                       |
|Temporal | previous-day               | dateTime-to-previous-day              |
|Temporal | previous-month             | dateTime-to-previous-month            |
|Temporal | previous-year              | dateTime-to-previous-year             |
|Temporal | set-time                   | dateTime-to-set-time                  |
|Temporal | utc-to-local               |                                       |
|Text     | empty-to-null              |                                       |
|Text     | first-chars                | text-to-first-chars                   |
|Text     | html-to-text               |                                       |
|Text     | last-chars                 | text-to-last-chars                    |
|Text     | length                     | text-to-length                        |
|Text     | lower                      | text-to-lower                         |
|Text     | mask-to-text               |                                       |
|Text     | null-to-empty              |                                       |
|Text     | pad-left                   | text-to-pad-left                      |
|Text     | pad-right                  | text-to-pad-right                     |
|Text     | prefix                     | text-to-prefix                        |
|Text     | remove-chars               | text-to-remove-chars                  |
|Text     | skip-first-chars           | text-to-skip-first-chars              |
|Text     | skip-last-chars            | text-to-skip-last-chars               |
|Text     | suffix                     | text-to-suffix                        |
|Text     | text-to-after              | text-to-text-to-after                 |
|Text     | text-to-before             | text-to-text-to-before                |
|Text     | text-to-datetime           |                                       |
|Text     | text-to-html               |                                       |
|Text     | text-to-mask               |                                       |
|Text     | token                      | text-to-token                         |
|Text     | token-count                | text-to-token-count                   |
|Text     | trim                       | text-to-trim                          |
|Text     | upper                      | text-to-upper                         |
|Text     | whitespaces-to-empty       | blank-to-empty                        |
|Text     | whitespaces-to-null        | blank-to-null                         |
|Text     | without-diacritics         | text-to-without-diacritics            |
|Text     | without-whitespaces        | text-to-without-whitespaces           |
<!-- END FUNCTION TABLE -->

### Predicates
<!-- START PREDICATE TABLE -->
|Scope    | Name                           | Aliases                               |
|-------- | ------------------------------ | --------------------------------------|
|Boolean  | false                          | boolean-is-false                      |
|Boolean  | false-or-null                  | boolean-is-false-or-null              |
|Boolean  | identical-to                   | boolean-is-identical-to               |
|Boolean  | true                           | boolean-is-true                       |
|Boolean  | true-or-null                   | boolean-is-true-or-null               |
|Numeric  | equal-to                       | numeric-is-equal-to                   |
|Numeric  | even                           | numeric-is-even                       |
|Numeric  | greater-than                   | numeric-is-greater-than               |
|Numeric  | greater-than-or-equal          | numeric-is-greater-than-or-equal      |
|Numeric  | integer                        | numeric-is-integer                    |
|Numeric  | less-than                      | numeric-is-less-than                  |
|Numeric  | less-than-or-equal             | numeric-is-less-than-or-equal         |
|Numeric  | modulo                         | numeric-is-modulo                     |
|Numeric  | odd                            | numeric-is-odd                        |
|Numeric  | within-interval                | numeric-is-within-interval            |
|Numeric  | zero-or-null                   | numeric-is-zero-or-null               |
|Special  | null                           | is-null                               |
|Temporal | after                          | dateTime-is-after                     |
|Temporal | after-or-same-instant          | dateTime-is-after-or-same-instant     |
|Temporal | before                         | dateTime-is-before                    |
|Temporal | before-or-same-instant         | dateTime-is-before-or-same-instant    |
|Temporal | contained-in                   | dateTime-is-contained-in              |
|Temporal | on-the-day                     | dateTime-is-on-the-day                |
|Temporal | on-the-hour                    | dateTime-is-on-the-hour               |
|Temporal | on-the-minute                  | dateTime-is-on-the-minute             |
|Temporal | same-instant                   | dateTime-is-same-instant              |
|Text     | any-of                         | text-is-any-of                        |
|Text     | contains                       | text-contains                         |
|Text     | empty                          | text-is-empty                         |
|Text     | empty-or-null                  | text-is-empty-or-null                 |
|Text     | ends-with                      | text-ends-with                        |
|Text     | equivalent-to                  | text-is-equivalent-to                 |
|Text     | lower-case                     | text-is-lower-case                    |
|Text     | matches-date                   | text-matches-date                     |
|Text     | matches-datetime               | text-matches-datetime                 |
|Text     | matches-numeric                | text-matches-numeric                  |
|Text     | matches-regex                  | text-matches-regex                    |
|Text     | matches-time                   | text-matches-time                     |
|Text     | sorted-after                   | text-is-sorted-after                  |
|Text     | sorted-after-or-equivalent-to  | text-is-sorted-after-or-equivalent-to |
|Text     | sorted-before                  | text-is-sorted-before                 |
|Text     | sorted-before-or-equivalent-to | text-is-sorted-before-or-equivalent-to|
|Text     | starts-with                    | text-starts-with                      |
|Text     | upper-case                     | text-is-upper-case                    |
<!-- END PREDICATE TABLE -->


























