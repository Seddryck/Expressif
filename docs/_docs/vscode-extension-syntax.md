---
title: VSCode extension syntax highlighting
subtitle: Installing and using
tags: [edition]
---
# VSCode Extension Syntax Highlighting

Expressif provides a Visual Studio Code extension (VSIX) for syntax highlighting of Expressif expressions. This extension makes it easier to read and write Expressif code in VSCode by providing colorization and improved readability.

Once installed, Expressif files and code blocks will be highlighted automatically.

## Prerequisites

Install [Visual Studio Code](https://code.visualstudio.com)

## Download extension

Extension is currently not available on the VSCode Marketplace but you can download it from GitHub.

You can download the latest `.vsix` release from the [Releases page](https://github.com/Seddryck/Expressif/releases) of this repository. Look for assets named like `expressif-syntax-*.vsix`.


## Install VS Code Extension

1. Open Visual Studio Code.
2. Go to the Extensions view (`Ctrl+Shift+X`).
3. Click the three-dot menu in the top right and select `Install from VSIX...`.
4. Browse to the downloaded `.vsix` file and select it.
5. Reload VSCode if prompted.

## Using the extension

Once the extension is installed, Visual Studio Code automatically detects Expressif files based on their extensions (`.expressif` or `.expr`) and applies the Expressif syntax highlighting rules defined by the extension.

This means that:

- keywords receive dedicated colors (mostly pipes)
- functions and predicates are highlighted distinctly
- strings and numbers are styled automatically
- constants are styled automatically (`null`, `empty`, `blank` ...)
- brackets and quotes benefit from editor support
- comments are recognized properly

For example, a file named `calculation.expr` will immediately use the Expressif language mode when opened in Visual Studio Code