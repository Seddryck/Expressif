---
title: Tooling
nav_order: 4
has_children: true
permalink: /tooling/
---

Visual Studio Code is the primary editor for Expressif. It has the most direct installation path and can combine syntax highlighting with language-aware diagnostics and completion.

Expressif also aims to support other editors and publishing tools wherever their extension model allows it. Portable TextMate, Notepad++, Rouge, and Language Server Protocol integrations make the same language easier to work with outside VS Code.

## Choose your VS Code experience

VS Code users have two levels of support:

| Option | Install | What you get |
|:-------|:--------|:-------------|
| Syntax highlighting | [Expressif Syntax]({{ '/tooling/textmate-grammar/' | relative_url }}) VSIX | TextMate colorization, `.expr` and `.expressif` file recognition, and basic bracket and quote behavior. |
| Complete editor support | Expressif Syntax VSIX **and** [Expressif Language Support]({{ '/tooling/language-server/' | relative_url }}) VSIX | Syntax highlighting plus live syntax diagnostics and function completion. |

The two extensions are complementary. Expressif Language Support embeds and starts the language server, but it does not contain the TextMate grammar; Expressif Syntax supplies the colorization. Install both for the complete experience.

VS Code users do **not** need to download the standalone server unless they deliberately want to test or override the bundled server with another build.

## Support outside VS Code

| Integration | Distribution | Capabilities |
|:------------|:-------------|:-------------|
| [Expressif Language Server]({{ '/tooling/language-server/' | relative_url }}) | Standalone LSP server | Live syntax diagnostics and function completion in Zed and other LSP-capable editors. |
| [TextMate grammar]({{ '/tooling/textmate-grammar/' | relative_url }}) | `expressif-<version>.tmLanguage.json` | Syntax highlighting for TextMate consumers such as Shiki, or Monaco through a tokenizer such as `vscode-textmate`. |
| [Notepad++]({{ '/tooling/notepad-plus-plus/' | relative_url }}) | `expressif-<version>-notepadpp-udl.xml` | Syntax highlighting through a user-defined language. |
| [Rouge]({{ '/tooling/rouge/' | relative_url }}) | `expressif-<version>-rouge.zip` | Syntax highlighting for Jekyll and other Ruby publishing tools. |

These integrations do not all provide the same features: editor grammars perform lexical colorization, while the language server parses the document and provides interactive assistance. An editor can use both when it supports both mechanisms.

Download the syntax highlighters from the matching Expressif version on the [Expressif Releases page](https://github.com/Seddryck/Expressif/releases). The language server is maintained and released separately in [Seddryck/Expressif.LanguageServer](https://github.com/Seddryck/Expressif.LanguageServer).
