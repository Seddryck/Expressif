---
name: scaffold
description: "Use when adding a new function, predicate, or accumulator definition; validates input, normalizes summaries and parameters, prepares branch/commit-message metadata, updates docs/_data JSON, and creates conformance YAML tests/cases without committing."
---

# /scaffold

Create a normalized documentation record for a function, predicate, or accumulator, then append it to the correct JSON catalog.

Also generate a conformance YAML file with normalized tests and cases for the new operator.

## Inputs Expected

Collect these values from the user (ask follow-up questions when missing):
- `kind`: `function` | `predicate` | `accumulator`
- `name`: canonical API name (kebab-case)
- `scope`: one of `Text`, `Numeric`, `Temporal`, `Special`, `Array`
- `summary`: behavior summary for the entry
- `parameters`: zero or more parameter definitions with
  - `name`
  - `optional` (`true` or `false`)
  - `summary`
- `aliases` (optional): if omitted, default to a single alias equal to `name`
- `isPublic` (optional): defaults to `true`
- `commitMessage` (optional): if omitted, generate one

## Output Target

Map `kind` to one JSON file:
- `function` -> `docs/_data/function.json`
- `predicate` -> `docs/_data/predicate.json`
- `accumulator` -> `docs/_data/accumulator.json`

Map conformance file path as:
- `function` -> `conformance/functions/<scope-lower>/<name>.yaml`
- `predicate` -> `conformance/predicates/<scope-lower>/<name>.yaml`
- `accumulator` -> `conformance/accumulators/<scope-lower>/<name>.yaml`

Where:
- `<scope-lower>` is lowercase scope (`text`, `numeric`, `temporal`, `special`, `array`).
- If the `<scope-lower>` folder does not exist, create it.

## Validation Checks

Run all checks before proposing a record:
- `name` is non-empty, lowercase kebab-case (`^[a-z0-9]+(?:-[a-z0-9]+)*$`).
- `scope` is exactly one of: `Text`, `Numeric`, `Temporal`, `Special`, `Array`.
- `summary` is non-empty after normalization.
- Parameter names are unique and kebab-case.
- Parameter summaries are non-empty after normalization.
- `aliases` are unique, kebab-case, and include `name` at least once.
- No duplicate entry in the target file for either:
  - same `Name`
  - any overlapping alias in `Aliases`
- Conformance target file does not already exist for the same `kind/scope/name` unless user explicitly requests overwrite.

If any check fails, explain the exact issue and ask for correction instead of editing files.

## Normalization Rules

Normalize text into a documentation-ready style:
- Keep `Name` and alias tokens unchanged except trim/normalize whitespace.
- Convert `scope` to PascalCase exactly as allowed values.
- Normalize summary:
  - Start with a clear present-tense verb phrase (for example: `Returns`, `Computes`, `Checks`, `Applies`).
  - Use a concise, objective sentence style.
  - End with a period.
  - Include graceful-failure behavior as a final sentence when relevant: `Returns \`null\` when the input cannot be evaluated.`
- Normalize parameter summaries:
  - One sentence each, concise and specific.
  - End with a period.
  - Describe the parameter role, not implementation details.
- Do not document exceptions; these APIs fail gracefully to `null`.

Normalize conformance assets:
- YAML header fields are ordered as:
  - `suite: <scope-lower>`
  - `kind: <kind>`
  - `operator: <name>`
  - `tests:`
- Keep `kind` lowercase (`function`, `predicate`, `accumulator`).
- Keep IDs lowercase kebab-case with dot-separated segments.

Repository findings to apply directly (do not rediscover):
- Conformance case values for special inputs use `(null)` for null and `(empty)` for empty; use `(blank)` only when the scenario explicitly targets blank text.
- For non-evaluable outputs that should be null, emit YAML null as an empty value on `expected:`.
- Existing `conformance/functions` scope folders may not include every scope. If a required scope folder (for example `array`) is missing, create it as part of the edit.

Search prohibition when rules are already defined:
- If this skill already defines a rule or convention, treat it as authoritative and do not run exploratory repository-wide search or regex to rediscover it.
- Prohibited for rediscovery: broad `grep`, `file_search`, or semantic scans for formatting, naming, or YAML style already specified by this skill.
- Allowed searches are limited to required validation checks only: duplicate `Name` or alias in the target JSON file, and whether the target conformance YAML already exists.
- If required behavior is still ambiguous after applying this skill, stop and ask the user instead of searching broadly.

## Defaulting Rules

When omitted:
- `IsPublic` -> `true`
- `Aliases` -> `[name]`
- `Parameters` -> `[]`

Emit JSON using this shape:

```json
{
  "Name": "<name>",
  "IsPublic": true,
  "Aliases": ["<alias-1>"],
  "Scope": "<Scope>",
  "Summary": "<normalized summary>",
  "Parameters": [
    {
      "Name": "<parameter-name>",
      "Optional": false,
      "Summary": "<normalized parameter summary>"
    }
  ]
}
```

Emit conformance YAML using this shape:

```yaml
suite: <scope-lower>
kind: <kind>
operator: <name>
tests:
  - id: <test-id>
    cases:
      - id: <case-id>
        value: <input>
        expected: <expected-output>
        parameters:
          - <p1>
          - <p2>
```

If a test has no parameters, omit `parameters` from cases.

## Conformance Test Rules

Generate tests and cases in addition to the JSON record.

Test IDs:
- Base pattern: `name-of-function.valid`.
- If the operator has parameters, test ID must include parameter markers in order:
  - `name-of-function.valid.parameter-one`
  - `name-of-function.valid.parameter-one.parameter-two`
  - Optional parameters must be marked with `(optional)` in the test ID proposal shown to the user, then normalized in YAML ID as `optional-<parameter-name>`.

Case IDs:
- Start with the full test ID.
- Append input type segment and scenario segment:
  - `<test-id>.<input-type>.<scenario>`
- Input type segments:
  - `text`, `numeric`, `special`
  - temporal-specific when needed: `date`, `date-time`

Case coverage and count:
- Each generated test must contain between 4 and 12 cases.
- Always include at least:
  - `<test-id>.special.null`
  - `<test-id>.special.empty`
- Add representative domain cases based on scope and behavior. Typical scenario labels include:
  - numeric: `negative`, `zero`, `positive`, `integer`, boundaries
  - temporal: `before`, `equal`, `after`, date/date-time variants
  - text: `match`, `non-match`, `case-variant`, `blank`

Expected output policy:
- Compute `expected` from the operator semantics in the normalized summary and parameter definitions.
- If expected outputs cannot be derived unambiguously from provided semantics, stop and ask the user for explicit expected outputs or concrete behavior examples before generating YAML cases.
- For functions/accumulators that fail gracefully, use YAML null for null/empty failures (`expected:` empty value).
- For predicates, use boolean outputs and generally `false` for non-evaluable null/empty inputs unless semantics explicitly state otherwise.
- Use deterministic values only; avoid random or clock-dependent expectations unless explicitly parameterized.

## Confirmation Gate

Before writing:
1. Show a visible, human-readable summary of the function/predicate/accumulator first (name, scope, behavior, cardinality, graceful failure behavior, and parameters).
2. Report checks as a single status line when all checks pass: `All checks are green.`
3. Only if checks fail, show the detailed failed checks and required corrections.
4. Show the fully normalized JSON record.
5. Show the fully normalized conformance YAML proposal.
6. Show both target files (JSON + YAML).
7. Ask for explicit confirmation (for example: `Confirm add? (yes/no)`).

Only continue on explicit confirmation.

## Git Workflow Rules

Before editing the JSON file, enforce branch policy:
- Check current branch with `git branch --show-current`.
- If current branch is `main`, create and switch to `feature/<name>`.
- If current branch is not `main`, keep current branch.

Commit message policy:
- If user already provided a commit message in this conversation, keep it.
- Otherwise generate and present: `docs(data): add <kind> definition for <name>`.
- Treat this as the selected commit message for later use.

Important:
- Never create a commit in this workflow.
- Never stage unrelated files.

## File Edit Rules

When confirmed and branch policy satisfied:
- Append the new record to the mapped JSON array.
- Preserve existing JSON formatting style in the file.
- Do not reorder existing entries unless user explicitly asks.
- Create the conformance YAML file at the mapped path.
- Ensure each test has 4 to 12 cases and includes `special.null` and `special.empty`.

## Final Response Checklist

After successful update, report:
- Branch used/created.
- Confirmation checks status (use `All checks are green.` when applicable).
- JSON file updated.
- Conformance YAML file created.
- Added `Name` and `Scope`.
- Commit message selected.
- Explicitly state that no commit was created.
