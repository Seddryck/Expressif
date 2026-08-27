---
title: Rouge
parent: Tooling
nav_order: 4
---

Expressif provides a standalone lexer for [Rouge](https://rouge-ruby.github.io/), the syntax highlighter used by Jekyll and other Ruby publishing tools.

## Download

Download `expressif-syntax-<version>-rouge.zip` from the matching version on the [GitHub Releases page](https://github.com/Seddryck/Expressif/releases), then extract the archive. It contains `expressif-rouge.rb`.

## Load the lexer

Make the lexer available to the Ruby process that performs syntax highlighting and require it before Rouge processes any code blocks:

```ruby
require_relative "expressif-rouge"
```

Rouge can then find the lexer by its `expressif` tag:

```ruby
lexer = Rouge::Lexer.find("expressif").new
```

The lexer also declares the `.expr` and `.expressif` filename patterns.

## Use with Jekyll

Place the lexer where your Jekyll configuration can load it before rendering. Once loaded, label fenced code blocks as `expressif`:

````markdown
```expressif
@value | trim | upper
```
````

The exact loading location depends on the site configuration. For a site that permits custom plugins, requiring the lexer from a file in `_plugins` is a convenient option.

## Example

```expressif
{1, 2, 3, 4}
| filter(greater-than(2))
```
