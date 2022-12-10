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
|Scope    | Name                               | Aliases                   |
|-------- | ---------------------------------- | --------------------------|
|IO       | file-to-creation-datetime          | creation-datetime         |
|IO       | file-to-creation-datetime-utc      | creation-datetime-utc     |
|IO       | file-to-size                       | size                      |
|IO       | file-to-update-datetime            | update-datetime           |
|IO       | file-to-update-datetime-utc        | update-datetime-utc       |
|IO       | path-to-directory                  | directory                 |
|IO       | path-to-extension                  | extension                 |
|IO       | path-to-filename                   | filename                  |
|IO       | path-to-filename-without-extension | filename-without-extension|
|IO       | path-to-root                       | root                      |
|Numeric  | null-to-zero                       |                           |
|Numeric  | numeric-to-add                     |                           |
|Numeric  | numeric-to-ceiling                 | ceiling                   |
|Numeric  | numeric-to-clip                    |                           |
|Numeric  | numeric-to-decrement               | decrement                 |
|Numeric  | numeric-to-divide                  | divide                    |
|Numeric  | numeric-to-floor                   | floor                     |
|Numeric  | numeric-to-increment               | increment                 |
|Numeric  | numeric-to-integer                 | integer                   |
|Numeric  | numeric-to-invert                  | invert                    |
|Numeric  | numeric-to-multiply                | multiply                  |
|Numeric  | numeric-to-round                   | round                     |
|Numeric  | numeric-to-subtract                |                           |
|Special  | any-to-any                         |                           |
|Special  | null-to-value                      |                           |
|Special  | value-to-value                     |                           |
|Temporal | datetime-to-add                    |                           |
|Temporal | datetime-to-ceiling-hour           | ceiling-hour              |
|Temporal | datetime-to-ceiling-minute         | ceiling-minute            |
|Temporal | datetime-to-clip                   |                           |
|Temporal | datetime-to-date                   | date                      |
|Temporal | datetime-to-first-of-month         | first-of-month            |
|Temporal | datetime-to-first-of-year          | first-of-year             |
|Temporal | datetime-to-floor-hour             | floor-hour                |
|Temporal | datetime-to-floor-minute           | floor-minute              |
|Temporal | datetime-to-last-of-month          | last-of-month             |
|Temporal | datetime-to-last-of-year           | last-of-year              |
|Temporal | datetime-to-next-day               | next-day                  |
|Temporal | datetime-to-next-month             | next-month                |
|Temporal | datetime-to-next-year              | next-year                 |
|Temporal | datetime-to-previous-day           | previous-day              |
|Temporal | datetime-to-previous-month         | previous-month            |
|Temporal | datetime-to-previous-year          | previous-year             |
|Temporal | datetime-to-set-time               | set-time                  |
|Temporal | datetime-to-subtract               |                           |
|Temporal | date-to-age                        | age                       |
|Temporal | invalid-to-date                    |                           |
|Temporal | local-to-utc                       |                           |
|Temporal | null-to-date                       |                           |
|Temporal | utc-to-local                       |                           |
|Text     | blank-to-empty                     |                           |
|Text     | blank-to-null                      |                           |
|Text     | empty-to-null                      |                           |
|Text     | html-to-text                       |                           |
|Text     | mask-to-text                       |                           |
|Text     | null-to-empty                      |                           |
|Text     | text-to-after                      | after                     |
|Text     | text-to-before                     | before                    |
|Text     | text-to-datetime                   |                           |
|Text     | text-to-first-chars                | first-chars               |
|Text     | text-to-html                       | html                      |
|Text     | text-to-last-chars                 | last-chars                |
|Text     | text-to-length                     | length                    |
|Text     | text-to-lower                      | lower                     |
|Text     | text-to-mask                       | mask                      |
|Text     | text-to-pad-left                   | pad-left                  |
|Text     | text-to-pad-right                  | pad-right                 |
|Text     | text-to-prefix                     | prefix                    |
|Text     | text-to-remove-chars               | remove-chars              |
|Text     | text-to-skip-first-chars           | skip-first-chars          |
|Text     | text-to-skip-last-chars            | skip-last-chars           |
|Text     | text-to-suffix                     | suffix                    |
|Text     | text-to-token                      | token                     |
|Text     | text-to-token-count                | token-count               |
|Text     | text-to-trim                       | trim                      |
|Text     | text-to-upper                      | upper                     |
|Text     | text-to-without-diacritics         | without-diacritics        |
|Text     | text-to-without-whitespaces        | without-whitespaces       |
<!-- END FUNCTION TABLE -->

### Predicates
<!-- START PREDICATE TABLE -->
|Scope    | Name                                   | Aliases                       |
|-------- | -------------------------------------- | ------------------------------|
|Boolean  | boolean-is-false                       | false                         |
|Boolean  | boolean-is-false-or-null               | false-or-null                 |
|Boolean  | boolean-is-identical-to                | identical-to                  |
|Boolean  | boolean-is-true                        | true                          |
|Boolean  | boolean-is-true-or-null                | true-or-null                  |
|Numeric  | numeric-is-equal-to                    | equal-to                      |
|Numeric  | numeric-is-even                        | even                          |
|Numeric  | numeric-is-greater-than                | greater-than                  |
|Numeric  | numeric-is-greater-than-or-equal       | greater-than-or-equal         |
|Numeric  | numeric-is-integer                     | integer                       |
|Numeric  | numeric-is-less-than                   | less-than                     |
|Numeric  | numeric-is-less-than-or-equal          | less-than-or-equal            |
|Numeric  | numeric-is-modulo                      | modulo                        |
|Numeric  | numeric-is-odd                         | odd                           |
|Numeric  | numeric-is-within-interval             | within-interval               |
|Numeric  | numeric-is-zero-or-null                | zero-or-null                  |
|Special  | special-is-null                        | null                          |
|Temporal | temporal-is-after                      | after                         |
|Temporal | temporal-is-after-or-same-instant      | after-or-same-instant         |
|Temporal | temporal-is-before                     | before                        |
|Temporal | temporal-is-before-or-same-instant     | before-or-same-instant        |
|Temporal | temporal-is-contained-in               | contained-in                  |
|Temporal | temporal-is-on-the-day                 | on-the-day                    |
|Temporal | temporal-is-on-the-hour                | on-the-hour                   |
|Temporal | temporal-is-on-the-minute              | on-the-minute                 |
|Temporal | temporal-is-same-instant               | same-instant                  |
|Text     | text-contains                          | contains                      |
|Text     | text-ends-with                         | ends-with                     |
|Text     | text-is-any-of                         | any-of                        |
|Text     | text-is-empty                          | empty                         |
|Text     | text-is-empty-or-null                  | empty-or-null                 |
|Text     | text-is-equivalent-to                  | equivalent-to                 |
|Text     | text-is-lower-case                     | lower-case                    |
|Text     | text-is-sorted-after                   | sorted-after                  |
|Text     | text-is-sorted-after-or-equivalent-to  | sorted-after-or-equivalent-to |
|Text     | text-is-sorted-before                  | sorted-before                 |
|Text     | text-is-sorted-before-or-equivalent-to | sorted-before-or-equivalent-to|
|Text     | text-is-upper-case                     | upper-case                    |
|Text     | text-matches-date                      | matches-date                  |
|Text     | text-matches-datetime                  | matches-datetime              |
|Text     | text-matches-numeric                   | matches-numeric               |
|Text     | text-matches-regex                     | matches-regex                 |
|Text     | text-matches-time                      | matches-time                  |
|Text     | text-starts-with                       | starts-with                   |
<!-- END PREDICATE TABLE -->















