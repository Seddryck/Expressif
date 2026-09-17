---
layout: docs
title: Configuration
parent: Command-line interface
nav_order: 9
permalink: /cli/configuration/
---

## OpenLineage reporting

Reporting is disabled by default. Set `OPENLINEAGE_URL` to a backend base URL to
report `run`, `evaluate`, and REPL executions through `Expressif.OpenLineage`.

```bash
OPENLINEAGE_URL=http://localhost:5000 \
OPENLINEAGE_NAMESPACE=my-application \
OPENLINEAGE_JOB_NAME=customers \
expressif run 'upper' --source customers.json
```

| Variable | Default | Purpose |
| --- | --- | --- |
| `OPENLINEAGE_URL` | unset | HTTP/HTTPS backend base URL; enables reporting. |
| `OPENLINEAGE_ENDPOINT` | `api/v1/lineage` | Relative endpoint appended to the base URL. |
| `OPENLINEAGE_API_KEY` | unset | Optional Bearer authentication token. |
| `OPENLINEAGE_NAMESPACE` | `expressif` | Stable job namespace. |
| `OPENLINEAGE_JOB_NAME` | command name | Stable job name (`run`, `evaluate`, or `repl`). |
| `OPENLINEAGE_DISABLED` | `false` | Set to `true` to disable reporting even with a URL. |

The HTTP variables follow the [OpenLineage simple HTTP configuration conventions](https://openlineage.io/docs/client/python/configuration/).
This minimal integration does not read `openlineage.yml` or the full nested
transport configuration supported by other OpenLineage clients.

Each command execution generates one `START` followed by `COMPLETE` or `FAIL`;
`run` reports one run for its entire row sequence. REPL reports each expression
evaluation. Events include the expression and Expressif version in a custom job
facet. JSON/CSV source files are identified by absolute file URIs, including
files selected using a format override. Expression-backed sources are not
identified as datasets because their underlying data locations are unknown.
Inline input, anonymous values, and stdout have no dataset identity. The CLI
cannot infer a shell redirection destination.

Events never appear on stdout. Configuration or delivery failures are reported
on stderr and preserve execution results and exit codes. Delivery is best effort
with a five-second timeout per HTTP request and no retries.

## Output configuration

Expressif reads `expressif.config.json` beside its executable. Use `expressif config path` to locate it. The packaged file contains the same defaults used when the file is absent:

```json
{
  "output-style": "compact",
  "indent": 2,
  "preferred-line-width": 80,
  "inline-types": [
    "tuple"
  ]
}
```

Set a shared preference or override it for `repl`, `run`, or `evaluate`:

```bash
expressif config set output-style compact
expressif config set indent 2
expressif config set preferred-line-width 100
expressif config set inline-types tuple,vector
expressif config set repl.output-style pretty
expressif config set repl.indent 4
expressif config set repl.inline-types none
expressif config get repl.output-style
expressif config list --command repl
expressif config unset repl.indent
```

An empty or whitespace-only file is treated as empty configuration, as is `{}`. An omitted setting, JSON `null`, or an empty/whitespace-only string inherits the next default independently. An empty command-specific setting inherits the shared setting, then the built-in default. Numeric `0` is a real indentation value and does not trigger fallback. For example, `{"output-style": "pretty", "indent": null}` enables pretty output with the built-in two-space indentation. Other malformed JSON and invalid nonempty values are errors. `config set` requires a valid value; use `config unset` to restore inheritance.

`get` returns the effective value, including inherited defaults. It works without a file and does not create one. `set` creates the file and parent directory when needed. `unset` removes an override, restoring inheritance. `list` shows the effective shared settings and their sources; `--command` selects a command's effective settings. Writes require permission to modify the executable directory.

Each setting resolves independently. When a command-line selector exists, it takes
precedence; otherwise resolution starts with the command-specific setting:

1. Explicit command-line option (`--output-style`, `--pretty`, `--compact`, or `--indent`).
2. Command-specific setting.
3. Shared setting.
4. Built-in defaults: `compact`, two spaces, a preferred width of 80, and `tuple` as the inline type.

For example, a file with `"indent": 2` and `"repl": { "output-style": "pretty" }` gives the REPL pretty output with two spaces while other commands remain compact.

`output-style` accepts `compact` or `pretty`. `indent` accepts an integer from 0 to 8 or `tab`. Stored indentation has no effect on compact output. An explicit `--indent` requires the effective style to be pretty, including a style inherited from configuration. Style selectors remain mutually exclusive.

`preferred-line-width` accepts any positive integer. It includes indentation and
text already written on the current line when deciding whether an eligible value
fits. `inline-types` accepts a comma-separated list through `config set`; supported
names are `array`, `tuple`, `vector`, `pair`, `group`, `record`, `dictionary`, and
`grouping`. Use `none` to disable inline values. In JSON, the setting is stored as
an array of names, so it can also be edited directly:

```json
{
  "preferred-line-width": 100,
  "inline-types": ["tuple", "vector"],
  "repl": {
    "inline-types": []
  }
}
```

These settings affect pretty output only. Each has the same shared and per-command
inheritance as the existing formatting settings.

```bash
expressif repl --pretty --indent tab
expressif run reverse --input '{1, 2}' --compact
```

A saved pretty preference can make `run` output span multiple lines. Scripts requiring one result per line should pass `--compact` explicitly.

Unknown keys and invalid values are rejected by `set` without changing the file. Malformed configuration is reported as an input/configuration error (exit code 2). Updates preserve unrelated JSON settings and replace the file atomically. The `install-local` skill preserves an existing config file even with `-Force`; a fresh installation receives the packaged defaults.
