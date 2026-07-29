---
name: implement
description: "Generate or insert C# operator class code from function.json or predicate.json metadata, preserving summaries/parameter text and applying Expressif class scaffolding rules."
---

# /implement

Generate operator implementation code for this repository.

If no file is selected, create a new file named `function` and place the generated code in it.

If a file is selected, insert the generated code into the selected file.

## Inputs Expected

Collect these values from the user (ask follow-up questions when missing):
- `kind`: `function` | `predicate`
- `name`: operator canonical name (kebab-case or alias)
- `scope`: one of `Text`, `Numeric`, `Temporal`, `Special`, `Array`
- `parameters`: parameter list from the corresponding JSON entry
- `summary`: operator summary from the corresponding JSON entry
- `targetFile` (implicit): selected file, if any

## Source Of Truth

Use documentation metadata as the source of truth:
- `docs/_data/function.json` for `kind=function`
- `docs/_data/predicate.json` for `kind=predicate`

Rules:
- The generated XML summary text must be exactly the same as in the corresponding JSON entry.
- XML `<param>` text must be exactly the same as in the corresponding JSON parameter summaries.
- Parameter names must match the corresponding JSON parameter names.

If the operator cannot be matched unambiguously in JSON (name or alias conflict), stop and ask the user to choose the intended entry.

## Naming Rules

Normalize names before code generation:
- Class name is PascalCase from the operator `name`.
- Constructor parameter names are camelCase (PascalCase without the first uppercase letter).
- Public property names are PascalCase versions of parameter names.

## Type And Constructor Rules

Generate constructors and properties with these rules:
- Every constructor parameter is wrapped in `Func<T>`, except enum parameters which are not wrapped.
- Every constructor parameter becomes a public get-only property.
- One primary constructor includes all parameters.
- If metadata includes default values for optional parameters, generate additional overload constructors that apply those defaults.

## Base Class And Override Rules

Class inheritance and override rules:
- The generated class must always inherit from `Base<Scope><KindRoot>` where:
  - `scope=Numeric`, `kind=function` -> `BaseNumericFunction`
  - `scope=Numeric`, `kind=predicate` -> `BaseNumericPredicate`
  - apply the same pattern for other scopes.
- Create only mandatory override members for the chosen base class.
- Add a scaffolded protected override body with a review marker comment:
  - `// TODO REVIEW Scaffold`

Use expression-bodied members when concise; otherwise use block bodies.

## Output Shape

The generated code should be structurally similar to this template:

```csharp
/// <summary>
/// <summary-from-json>
/// </summary>
public class <ClassName> : <BaseClass>
{
    public <TypeOrFuncType> <PropertyName> { get; }

    /// <param name="<ctorParam>"><param-summary-from-json></param>
    public <ClassName>(<CtorParameterList>)
        : base(...)
    {
        // Assign property values
    }

    protected override <ReturnType> <RequiredOverrideName>(<OverrideArgs>)
    {
        // TODO REVIEW Scaffold
        throw new NotImplementedException();
    }
}
```

## File Placement Rules

- If there is an active selected file, append or insert code into that file according to the user request.
- If there is no selected file, create a new file named `function` in the current workspace and write the generated code in it.
- Do not modify unrelated code blocks.

## Conformance Anchor Rules

After generating the function/predicate implementation, generate the conformance test anchor in `Expressif.Testing`.

Placement and structure:
- Test project is `Expressif.Testing`.
- Folder structure must mirror the implementation structure by kind and scope.
  - Function example: `Expressif.Testing/Functions/<Scope>/...`
  - Predicate example: `Expressif.Testing/Predicates/<Scope>/...`
- Test file structure should be equivalent to the function/predicate structure in the main project.

Class scaffold:
- Include these using directives:
  - `using Expressif.Functions.<Scope>;` for functions
  - `using Expressif.Predicates.<Scope>;` for predicates
  - `using Expressif.Testing.Conformance;`
- Namespace pattern:
  - Functions: `Expressif.Testing.Functions.<Scope>`
  - Predicates: `Expressif.Testing.Predicates.<Scope>`
- Class pattern:
  - `[TestFixture]`
  - `public class <GroupName>Test`

Method generation from conformance:
- Read tests from the operator conformance YAML (`conformance/functions/...` or `conformance/predicates/...`).
- For each test in `tests`, create one method anchor.
- Method name must be the test `id` converted to segmented PascalCase joined by underscores.
  - Example: `reverse.valid` -> `Reverse_Valid`.
- Method parameter order is always:
  1. input
  2. parameters (in YAML order)
  3. expected
- Each method must be decorated with `[Conformance]`.

Method body pattern:
- Use an expression-bodied assertion equivalent to:

```csharp
[Conformance]
public void FunctionName_Valid(string input, int param1, string expected)
    => Assert.That(new FunctionName(() => param1).Evaluate(input), Is.EqualTo(expected));
```

- For multiple constructor parameters, pass all in order.
- For predicates, assert against boolean expected values using the same `Assert.That(..., Is.EqualTo(expected))` pattern.

Typing guidance for method parameters:
- Use types inferred from conformance values and operator metadata.
- Keep `input` first and `expected` last even when nullable.

## Validation Checks

Run checks before writing:
- `kind` is `function` or `predicate`.
- `scope` is one of `Text`, `Numeric`, `Temporal`, `Special`, `Array`.
- A matching JSON entry exists in the chosen source file.
- Summary and parameter text are present and non-empty.
- Parameter names are unique.
- Class and property names are valid C# identifiers.

If any check fails, report the exact issue and ask for correction.

## Confirmation Gate

Before writing:
1. Show selected JSON source entry (name, scope, summary, parameters).
2. Show normalized class name and constructor signatures.
3. Show target file decision (`selected file` or `new file named function`).
4. Show code preview.
5. Ask for explicit confirmation: `Confirm implement? (yes/no)`.

Only continue on explicit confirmation.

## Edit Rules

When confirmed:
- Apply only the minimum edits needed to add the generated class.
- Preserve the file's indentation and style.
- Do not create commits.
- Do not stage files.

## Final Response Checklist

After successful update, report:
- Source JSON file used.
- Target file updated or created.
- Implemented class name.
- Constructors generated (including defaulted overloads, if any).
- Mandatory override scaffold added with `TODO REVIEW Scaffold`.
- Conformance test anchor file created or updated in `Expressif.Testing`.
- Conformance anchor methods generated from YAML test IDs.
- Explicitly state that no commit was created.
