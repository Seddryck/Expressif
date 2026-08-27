---
title: Expressif Language Server
parent: Tooling
nav_order: 1
---

[Expressif.LanguageServer](https://github.com/Seddryck/Expressif.LanguageServer) implements the Language Server Protocol (LSP) for Expressif. Unlike a syntax highlighter, it parses the document while you edit and can respond with language-aware feedback.

## Current capabilities

The language server currently provides:

- syntax diagnostics with source ranges;
- function completion, triggered while writing a pipeline or function name;
- full document synchronization over standard input and output.

Hover documentation, signature help, semantic highlighting, and navigation are planned but are not yet implemented. Basic colorization still comes from an editor grammar such as the [Expressif TextMate grammar]({{ '/tooling/textmate-grammar/' | relative_url }}).

## Choose an installation

| Setup | Install the standalone server? |
|:------|:-------------------------------|
| Expressif Language Support VSIX | **No.** The VSIX contains and starts its own self-contained server. |
| Zed or another LSP-capable editor | **Yes.** Install the server and configure the editor to launch it. |
| VS Code with a specific external server build | **Yes.** Install that server and set `expressif.languageServer.path`. |

The VSIX and standalone server are alternative installations for normal VS Code use. You do not need both.

## Use with Visual Studio Code

The language-server repository includes a thin VS Code client in its `vscode-extension` directory. The client registers `.expressif` files, starts the server, and forwards editor activity over LSP.

The packaged client embeds a self-contained server for its target platform. Installing the VSIX is therefore sufficient:

```console
code --install-extension ./artifacts/expressif-language-support.vsix
```

Leave `expressif.languageServer.path` empty to use the bundled server. Set it to the absolute path of a separately installed executable only when you intentionally want the VSIX to use that external server build.

The language-server client and the syntax-only `expressif-syntax-<version>-vscode.vsix` are complementary extensions. The language-support extension does not bundle the TextMate grammar:

| Extension | Purpose |
|:----------|:--------|
| Expressif Language Support | Runs the language server for diagnostics and completion. |
| Expressif Syntax | Provides the TextMate colorizer and basic editor configuration. |

Install both for the complete VS Code experience. Install only Expressif Syntax when colorization is sufficient and you do not want to run a language server.

## Use with Zed or another LSP client

The standalone installation is primarily intended for editors such as Zed, which can connect to an external language server but cannot install the VS Code VSIX. It can also be used to override the server bundled with the VSIX during development or testing.

Download the package for your platform from the [Expressif.LanguageServer Releases page](https://github.com/Seddryck/Expressif.LanguageServer/releases). Extract it, then configure Zed or your preferred LSP client to launch the server executable over standard input and output.

The current release package targets Windows x64 and .NET 10. Consult the release assets for the exact filename and supported platform before downloading.
