---
title: TextMate grammar
parent: Tooling
nav_order: 2
tags: [edition]
---
Expressif syntax highlighting is defined by a TextMate grammar. The standalone `expressif-syntax-<version>-textmate.json` file can be used in two main ways:

1. Use it with TextMate-compatible consumers such as Shiki.
2. Use it in Monaco-based applications through a TextMate tokenizer such as `vscode-textmate`; Monaco does not load TextMate grammars directly.

Custom editors, previewers, and language tooling can also use the grammar when they implement or integrate a compatible TextMate tokenizer.

Zed does not use TextMate grammars. Use the [Expressif Language Server]({{ '/tooling/language-server/' | relative_url }}) to add Expressif support to Zed.

## Download the standalone grammar

Download `expressif-syntax-<version>-textmate.json` from the matching version on the [GitHub Releases page](https://github.com/Seddryck/Expressif/releases), then follow the host tool's instructions for registering a TextMate grammar with the `source.expressif` scope.

The standalone file provides lexical colorization only. File-extension registration and editor behavior depend on the host tool.

## Use with Shiki

Install [Shiki](https://shiki.style/guide/install):

```shell
npm install shiki
```

Load the downloaded grammar as a custom language, then use its registered name when rendering an expression:

```javascript
import { readFile } from 'node:fs/promises'
import { createHighlighter } from 'shiki'

const grammar = JSON.parse(
  await readFile('./expressif.tmLanguage.json', 'utf8')
)

const highlighter = await createHighlighter({
  langs: [{ ...grammar, name: 'expressif' }],
  themes: ['github-dark'],
})

const expression = `filter(greater-than(15))
|> add(10)
| scan(sum)
| filter(less-than(150)
|OR greater-than(0))`

const html = highlighter.codeToHtml(expression, {
  lang: 'expressif',
  theme: 'github-dark',
})
```

The resulting `html` value contains a highlighted `<pre>` element:

![Expressif pipeline rendered by Shiki]({{ '/assets/images/shiki-expressif-rendering.svg' | relative_url }})

The grammar declares `Expressif` as its name. Overriding that property while registering it provides the predictable lowercase language identifier used by the rendering call without modifying the downloaded file.

## Use with Visual Studio Code

For Visual Studio Code, the grammar is distributed in a VSIX package that combines three pieces:

- the basic TextMate colorizer (`expressif.tmLanguage.json`);
- the Expressif language registration for `.expr` and `.expressif` files;
- editor configuration for brackets, quotes, and surrounding pairs.

The colorizer is the syntax-highlighting component. The VSIX is the convenient installable bundle for VS Code; VS Code users do not also need the standalone grammar.

The TextMate grammar performs lexical colorization only; it does not parse expressions or provide completion. For live diagnostics and completion, use the [Expressif Language Server]({{ '/tooling/language-server/' | relative_url }}) and its VS Code client.

Once installed, Expressif files and code blocks will be highlighted automatically.

### Prerequisites

Install [Visual Studio Code](https://code.visualstudio.com).

### Download the extension

The extension is not currently available on the Visual Studio Marketplace. Download `expressif-syntax-<version>-vscode.vsix` from the matching version on the [GitHub Releases page](https://github.com/Seddryck/Expressif/releases).


### Install the extension

1. Open Visual Studio Code.
2. Go to the Extensions view (`Ctrl+Shift+X`).
3. Open the Extensions view menu and select **Install from VSIX...**.
4. Browse to the downloaded `.vsix` file and select it.
5. Reload VS Code if prompted.

### Use the extension

Once the extension is installed, Visual Studio Code automatically detects Expressif files based on their extensions (`.expressif` or `.expr`) and applies the Expressif syntax highlighting rules defined by the extension.

This means that:

- operators such as pipes receive dedicated colors;
- functions, predicates, and accumulators are highlighted;
- strings, dates, and numbers are styled automatically;
- constants such as `null`, `empty`, and `blank` are recognized;
- references, brackets, and quotes benefit from editor support.

For example, a file named `calculation.expr` automatically uses the Expressif language mode when opened in Visual Studio Code.
