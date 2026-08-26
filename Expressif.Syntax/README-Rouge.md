# Expressif syntax highlighting for Rouge

[Expressif](https://github.com/Seddryck/Expressif) is a compact expression language for transforming, validating, and aggregating values. Its composable syntax makes complex data transformations easy to express and reuse.

This package provides syntax highlighting for Expressif through [Rouge](https://github.com/rouge-ruby/rouge).

## Contents

- `expressif-rouge.rb` — the Expressif lexer for Rouge.

## Installation

Download the Rouge package from the matching Expressif release and extract it.

The package contains a standalone lexer that loads Rouge when the lexer file is required. Load it from the Ruby application that performs syntax highlighting:

```ruby
require_relative "expressif-rouge"
```

Rouge can then find and instantiate the lexer using its `expressif` tag:

```ruby
lexer = Rouge::Lexer.find("expressif").new
```

It also declares support for Expressif files using the `*.expr` and `*.expressif` extensions.

### Jekyll

If your Jekyll site uses Rouge, make the lexer file available to the Jekyll process and require it before Rouge performs syntax highlighting. The exact loading location depends on how the site is configured.

Once loaded, Expressif code blocks can use:

````markdown
```expressif
@value | trim | upper
```
````

## More information

- Expressif repository: https://github.com/Seddryck/Expressif
- Expressif documentation: https://expressif.net/
- Rouge: https://github.com/rouge-ruby/rouge
