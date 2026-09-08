---
layout: docs
title: Interactive REPL
parent: Command-line interface
nav_order: 55
description: Evaluate expressions interactively and undo previous submissions.
---

Start an interactive session with `expressif` (no arguments) in a terminal, or with
`expressif repl`. Double-clicking `expressif.exe` on Windows also starts an
interactive session. A no-argument interactive launch displays a short welcome
with help and exit instructions and stays open until you exit.

To run commands from PowerShell, open a terminal in the executable's folder:

```powershell
.\expressif.exe
.\expressif.exe --help
.\expressif.exe evaluate '2 | add(1)'
```

The first command starts the REPL; exit with Ctrl+C or Ctrl+D on an empty line
before running the next command. Explicit commands print their output and exit,
leaving PowerShell open so you can read the result.

Automatic REPL startup requires both standard input and standard output to be
connected to an interactive terminal. With either redirected, or no terminal
available, `expressif` without arguments prints usage and exits without waiting
for input. Use an explicit `expressif repl` invocation when intentionally feeding
a session from redirected input.

Evaluate a standalone expression,
then start a line with `|` to apply another pipeline to the current result.

Use `expressif repl --output-style pretty --indent 4` to format every result with
four-space indentation. `--style-output` is an alias for `--output-style`;
`--pretty` and `--compact` are shortcuts for the corresponding styles. Choose
only one style selector. The default is compact output.

`--indent` accepts a space count from `0` to `8`, or `tab`, and requires pretty
output. Pretty output defaults to two-space indentation. These settings apply
throughout the session, including standalone expressions, subsequent pipelines,
and values restored by undo.

```text
> 2
2
> | add(1) | multiply(4)
12
> :undo
2
> | multiply(5)
10
```

Use **Up** and **Down** to browse expressions and commands submitted in the current
interactive session. Recalled input can be edited and runs only when you press
**Enter**. Down past the newest entry restores your unfinished input and cursor.
Blank submissions and consecutive identical entries are omitted from history.
History is kept only for the current session, including expressions that failed.

`:undo` restores and displays the result before the last successful submission.
One undo removes the entire submitted line, even when it contains several operators.
The next pipeline uses the restored value. Saved results are restored directly,
without evaluating the previous expressions again. Failed submissions do not add
undo steps, and `#null` is a valid saved result.

Repeated undo walks back through successful submissions, including standalone
expressions. Undoing the first submission reports `There is no current input.`;
evaluate a standalone expression before using another pipeline. When history is
empty, the command reports `Nothing to undo.`

In an interactive terminal, **Ctrl+Z** on an empty input line runs `:undo`
immediately, without Enter. While the line contains text, Ctrl+Z undoes the last
text edit instead. Use Left/Right, Home/End, Backspace, and Delete to edit the line.
Ctrl+C ends the session; Ctrl+D on an empty line also exits.

With redirected input or output, use the literal `:undo` command on its own line;
keyboard shortcuts are available only in an interactive terminal.

Output preferences can also be saved in [configuration](configuration.md), with `repl.output-style` and `repl.indent` overriding shared defaults. Explicit CLI options, including `--style-output`, override configured values.
