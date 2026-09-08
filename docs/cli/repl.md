---
layout: docs
title: Interactive REPL
parent: Command-line interface
nav_order: 55
description: Evaluate expressions interactively and undo previous submissions.
---

Start an interactive session with `expressif repl`. Evaluate a standalone expression,
then start a line with `|` to apply another pipeline to the current result.

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
