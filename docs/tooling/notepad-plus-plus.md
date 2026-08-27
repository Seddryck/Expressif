---
title: Notepad++
parent: Tooling
nav_order: 3
---

Expressif provides a Notepad++ user-defined language (UDL). It adds syntax highlighting for Expressif without requiring a Notepad++ plugin.

## Download

Download `expressif-<version>-notepadpp-udl.xml` from the matching version on the [GitHub Releases page](https://github.com/Seddryck/Expressif/releases).

## Install the language definition

1. Open Notepad++.
2. Select **Language > User Defined Language > Define your language**.
3. Select **Import**, then choose the downloaded XML file.
4. Restart Notepad++ if Expressif does not immediately appear in the **Language** menu.

Notepad++ automatically applies the language definition to `.expr` and `.expressif` files. You can also select **Expressif** from the **Language** menu for a file with another extension.

The UDL highlights functions, predicates, accumulators, constants, operators, strings, and numbers. Its colors follow your Notepad++ theme and can be customized from the user-defined language dialog.

## Example

<img
  src="{{ '/assets/images/notepad-plus-plus-syntax-highlighting.png' | relative_url }}"
  alt="An Expressif expression with syntax highlighting in Notepad++"
  width="787"
  height="873">
