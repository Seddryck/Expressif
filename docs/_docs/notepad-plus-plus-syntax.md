---
title: Notepad++ syntax highlighting
subtitle: Importing the generated user-defined language
tags: [edition]
---
# Notepad++ Syntax Highlighting

Expressif releases include a generated Notepad++ User Defined Language (UDL) file. The file highlights `.expr` and `.expressif` files using the functions, predicates, accumulators, literals, operators, and delimiters known to the version of Expressif in that release.

## Install the language

1. Download `expressif-<version>-notepadpp-udl.xml` from the [Expressif releases page](https://github.com/Seddryck/Expressif/releases).
2. In Notepad++, select **Language > User Defined Language > Define your language**.
3. Select **Import**, choose the downloaded XML file, and confirm the import.
4. Restart Notepad++.

Notepad++ then selects Expressif automatically for `.expr` and `.expressif` files. You can also select **Language > Expressif** manually.

The XML file is generated during the build from the same introspection output used by the documentation and Visual Studio Code extension. It does not contain a separately maintained list of library functions.
