---
name: implement
description: "Generate or insert C# implementation code for functions, predicates, and accumulators from repository metadata, preserving documentation text, conformance anchors, and Expressif implementation conventions, including specialized builders for syntax-driven functions."
---

# /implement

Generate operator implementation code for this repository.

Supported operator kinds:

* `function`
* `predicate`
* `accumulator`

If no file is selected and no family of functions/predicates/accumulators is fit for purpose, create a new file with suffix `Functions`, `Predicates` or `Accumulators` and place the generated code in it.

If a file is selected or family of functions is fit for purpose, insert the generated code into this file.

## Inputs Expected

Collect these values from the user (ask follow-up questions when missing):

* `kind`: `function` | `predicate` | `accumulator`
* `name`: operator canonical name (kebab-case or alias)
* `scope`: one of `Text`, `Numeric`, `Temporal`, `Special`, `Array`, `Tuple`, `Record`, `IO`
* `family` (optional): a logical group of functions
* `parameters`: parameter list from the corresponding JSON entry
* `summary`: operator summary from the corresponding JSON entry
* `targetFile` (implicit): selected file, if any

For functions, also determine the implementation mode:

* `regular`: normal metadata-driven function construction
* `syntax-driven`: function requires specialized parsing or binding semantics

Do not ask the user to choose the implementation mode when it can be determined from the repository or description.

## Source Of Truth

Use documentation metadata as the source of truth:

* `docs/_data/function.json` for `kind=function`
* `docs/_data/predicate.json` for `kind=predicate`
* `docs/_data/accumulator.json` for `kind=accumulator`

Rules:

* The generated XML summary text must be exactly the same as in the corresponding JSON entry.
* XML `<param>` text must be exactly the same as in the corresponding JSON parameter summaries.
* Parameter names must match the corresponding JSON parameter names.
* Pay attention to special characters such as `>`, `<` or `&` as they probably need to be encoded in XML comments

If the operator cannot be matched unambiguously in JSON (name or alias conflict), stop and ask the user to choose the intended entry.

For syntax-driven functions, metadata remains authoritative for public documentation, but parser and binding behavior already present in the repository is authoritative for construction semantics.

## Naming Rules

Normalize names before code generation.

For functions and predicates:

* Class name is PascalCase from the operator `name`.
* Constructor parameter names are camelCase (PascalCase without the first uppercase letter).
* Public property names are PascalCase versions of parameter names.

For accumulators:

* Class name is PascalCase from the operator `name` followed by `Accumulator`.

  * Example: `sum` -> `SumAccumulator`
  * Example: `first` -> `FirstAccumulator`
* Accumulators are registered using `[Accumulator(...)]` according to existing repository conventions.

For syntax-driven function builders:

* Runtime function class keeps the normal PascalCase function name.
* Builder class is `<ClassName>FunctionBuilder`.

  * Example: `coalesce` -> `CoalesceFunctionBuilder`
  * Example: `record` -> `RecordFunctionBuilder`
  * Example: `adjacent` -> `AdjacentFunctionBuilder`

## Determine Implementation Mode

Before generating a function implementation, determine whether normal constructor scaffolding is sufficient.

A function is syntax-driven when its construction requires behavior such as:

* dedicated parser parameter types;
* interpretation of syntax rather than ordinary values;
* deferred expression evaluation;
* short-circuit candidates;
* spread or structured-entry handling;
* open-expression compilation;
* shorthand expansion;
* lexical binding;
* tuple projection binding;
* function-specific parameter validation that cannot be expressed by normal constructor binding.

Existing examples include:

* `record`
* `coalesce`
* `adjacent`

An unusual constructor signature alone does not make a function syntax-driven.

If normal metadata-driven binding is sufficient, use the regular implementation path.

If specialized binding semantics are required, use the syntax-driven function path. Request confirmation from user before performing this choice.

## Type And Constructor Rules

### Regular functions and predicates

Generate constructors and properties with these rules:

* Every constructor parameter is wrapped in `Func<T>`, except enum parameters which are not wrapped.
* Every constructor parameter becomes a public get-only property.
* One primary constructor includes all parameters.
* If metadata includes default values for optional parameters, generate additional overload constructors that apply those defaults.

### Accumulators

Accumulators inherit from `BaseAccumulator`.

Current accumulator construction through `AccumulatorFactory` requires a public parameterless constructor. Do not generate constructor parameters for an accumulator unless the repository's accumulator factory has first been changed to support them. Request confirmation from users before applying this kind of modification.

Accumulator state belongs in private fields.

Implement the accumulator lifecycle through:

```csharp
public override void Initialize();
public override void Accumulate(object? item);
public override object? GetValue();
```

Use `Initialize()` to reset all mutable accumulator state.

Do not rely on constructor execution to reset per-aggregation state.

### Syntax-driven functions

Do not force syntax-driven functions through the regular `Base<Scope>Function` constructor scaffolding rules when these rules do not represent the required runtime design.

Separate:

1. runtime evaluation behavior;
2. parser-to-runtime construction/binding behavior.

The runtime function should remain independent from parser-specific types whenever possible.

Specialized construction belongs in an `IFunctionBuilder` implementation.

A builder should be responsible for:

* validating function-specific parsed parameters;
* interpreting dedicated parser parameter types;
* compiling nested or open expressions;
* creating deferred evaluators/providers;
* expanding shorthand when required;
* applying function-specific lexical or binding rules;
* creating the corresponding `IFunction`.

A builder should not:

* evaluate expressions during construction unless semantics explicitly require it;
* duplicate runtime evaluation behavior;
* expose parser types from the runtime function;
* duplicate the generic function-instantiation mechanism.

When specialized builders need regular expression construction, use the repository's builder-context abstraction rather than depending directly on the concrete `ExpressionFactory`.

## Base Class And Override Rules

### Regular functions and predicates

Class inheritance and override rules:

* The generated class must inherit from `Base<Scope><KindRoot>` where:

  * `scope=Numeric`, `kind=function` -> `BaseNumericFunction`
  * `scope=Numeric`, `kind=predicate` -> `BaseNumericPredicate`
  * apply the same pattern for other scopes.
* Create only mandatory override members for the chosen base class.
* Add a scaffolded protected override body with a review marker comment:

  * `// TODO REVIEW Scaffold`

Use expression-bodied members when concise; otherwise use block bodies.

### Accumulators

Accumulator implementations inherit from:

```csharp
BaseAccumulator
```

Generate the mandatory accumulator members:

```csharp
public override void Initialize()
{
    // TODO REVIEW Scaffold
}

public override void Accumulate(object? item)
{
    // TODO REVIEW Scaffold
}

public override object? GetValue()
{
    // TODO REVIEW Scaffold
    throw new NotImplementedException();
}
```

When an implementation has an obvious neutral initial value, `Initialize()` may assign it directly instead of containing a throw.

Do not introduce scope-specific accumulator base classes unless they already exist or the user confirmed that it could be created.

### Syntax-driven functions

Do not require `Base<Scope>Function` inheritance if existing runtime semantics require direct `IFunction` implementation or another established base class.

Preserve the most appropriate existing runtime abstraction.

Generate only the runtime members required by that abstraction.

The specialized builder is additional construction infrastructure and does not replace the runtime function itself.

## Output Shape

### Regular function or predicate

Generated code should be structurally similar to:

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

### Accumulator

Generated code should be structurally similar to:

```csharp
/// <summary>
/// <summary-from-json>
/// </summary>
[Accumulator(prefix: "", aliases: ["<name>"])]
public class <ClassName>Accumulator : BaseAccumulator
{
    // Private accumulator state

    public override void Initialize()
    {
        // Initialize/reset state
    }

    public override void Accumulate(object? item)
    {
        // TODO REVIEW Scaffold
    }

    public override object? GetValue()
    {
        // TODO REVIEW Scaffold
        throw new NotImplementedException();
    }
}
```

Use aliases from metadata when available rather than assuming only the canonical name.

### Syntax-driven function

The runtime class should contain only runtime semantics.

Its builder should be structurally similar to:

```csharp
public sealed class <ClassName>FunctionBuilder : IFunctionBuilder
{
    public string Name => "<name>";

    public IFunction Build(
        Parsers.Function function,
        IFunctionBuildContext context)
    {
        // Validate parser parameter shape
        // Interpret function-specific syntax
        // Build deferred expressions/providers where required
        // TODO REVIEW Scaffold

        return new <ClassName>(...);
    }
}
```

Adapt the exact interface to the builder abstraction currently present in the repository.

Do not invent a parallel builder architecture if one already exists.

## File Placement Rules

* If there is an active selected file, append or insert code into that file according to the user request.
* If there is no selected file:

  * functions and predicates: create a new file named `function` in the current workspace;
  * accumulators: create a new file named `accumulator` in the current workspace.
* Do not modify unrelated code blocks.

For syntax-driven functions:

* place runtime behavior with the corresponding function implementation;
* place builder behavior according to the existing specialized-builder structure;
* do not move parser-specific construction logic into the runtime function merely to keep everything in one file.

For accumulators:

* follow the existing `Expressif/Accumulators` implementation structure;
* do not create an artificial scope namespace when accumulators are currently stored directly under `Expressif.Accumulators`.

## Conformance Anchor Rules

After generating the function, predicate, or accumulator implementation, generate the conformance test anchor in `Expressif.Testing`.

### Placement and structure

Test project is `Expressif.Testing`.

For functions and predicates, folder structure must mirror the implementation structure by kind and scope:

* Function:
  `Expressif.Testing/Functions/<Scope>/...`
* Predicate:
  `Expressif.Testing/Predicates/<Scope>/...`

For accumulators, use the repository's accumulator testing structure corresponding to:

```text
conformance/accumulators/<scope-lower>/<name>.yaml
```

Do not place accumulator tests under `Functions` merely because accumulators can be invoked through functions such as `fold`.

### Test class scaffold

Include these using directives as applicable:

```csharp
using Expressif.Functions.<Scope>;
using Expressif.Predicates.<Scope>;
using Expressif.Accumulators;
using Expressif.Testing.Conformance;
```

Use only the directives required for the selected kind.

Namespace patterns:

* Functions:
  `Expressif.Testing.Functions.<Scope>`
* Predicates:
  `Expressif.Testing.Predicates.<Scope>`
* Accumulators:
  follow the existing accumulator test namespace under `Expressif.Testing`

Class pattern:

```csharp
[TestFixture]
public class <GroupName>Test
```

### Method generation from conformance

Read tests from the operator conformance YAML:

* functions:
  `conformance/functions/...`
* predicates:
  `conformance/predicates/...`
* accumulators:
  `conformance/accumulators/...`

For each test in `tests`, create one method anchor.

Method name must be the test `id` converted to segmented PascalCase joined by underscores.

Example:

```text
reverse.valid
```

becomes:

```text
Reverse_Valid
```

### Function and predicate method parameters

Method parameter order is always:

1. input
2. parameters, in YAML order
3. expected

Each method must be decorated with `[Conformance]`.

Method body pattern:

```csharp
[Conformance]
public void FunctionName_Valid(
    string input,
    int param1,
    string expected)
    => Assert.That(
        new FunctionName(() => param1).Evaluate(input),
        Is.EqualTo(expected));
```

For multiple constructor parameters, pass all in order.

For predicates, assert against boolean expected values using the same:

```csharp
Assert.That(..., Is.EqualTo(expected))
```

pattern.

### Accumulator conformance anchors

Accumulator tests should exercise the accumulator lifecycle.

A generated anchor should conceptually:

1. instantiate the accumulator;
2. call `Initialize()`;
3. accumulate every input value;
4. compare `GetValue()` with expected.

For example:

```csharp
[Conformance]
public void Sum_Valid(
    object?[] input,
    object? expected)
{
    var accumulator = new SumAccumulator();
    accumulator.Initialize();

    foreach (var item in input)
        accumulator.Accumulate(item);

    Assert.That(
        accumulator.GetValue(),
        Is.EqualTo(expected));
}
```

Do not test accumulator behavior only indirectly through `Fold` when a direct accumulator conformance anchor can express the behavior.

### Syntax-driven function tests

Generate the normal conformance anchor from YAML.

In addition, when construction semantics are not fully represented by conformance cases, generate focused unit-test anchors for the specialized builder.

Relevant scenarios can include:

* invalid parser parameter shape;
* deferred construction/evaluation;
* short-circuit preservation;
* spread handling;
* shorthand expansion;
* open-expression construction;
* lexical binding;
* restoration of context after evaluation.

Do not duplicate tests already adequately covered by conformance.

### Typing guidance

Use types inferred from conformance values and operator metadata.

Keep `input` first and `expected` last even when nullable.

For accumulator YAML where the input represents multiple accumulated values, choose a collection type compatible with the conformance values and iterate over it.

## Validation Checks

Run checks before writing:

* `kind` is `function`, `predicate`, or `accumulator`.
* `scope` is one of `Text`, `Numeric`, `Temporal`, `Special`, `Array`.
* A matching JSON entry exists in the chosen source file.
* Summary and parameter text are present and non-empty.
* Parameter names are unique.
* Class and property names are valid C# identifiers.

For accumulators also validate:

* the generated class name ends in `Accumulator`;
* the implementation derives from `BaseAccumulator`;
* the implementation can be constructed parameterlessly under the current `AccumulatorFactory`;
* generated state can be reset through `Initialize()`.

For syntax-driven functions also validate:

* specialized binding is actually required;
* parser/binding semantics have been inspected before generating code;
* runtime behavior and construction behavior remain separated;
* parser-specific types do not leak unnecessarily into the runtime function;
* an existing specialized-builder abstraction is reused when available.

If any check fails, report the exact issue and ask for correction.

## Confirmation Gate

Before writing:

1. Show selected JSON source entry:

   * name
   * scope
   * summary
   * parameters
2. Show the selected implementation mode:

   * regular function/predicate
   * accumulator
   * syntax-driven function
3. Show normalized class name and constructor signatures.
4. For syntax-driven functions, also show:

   * runtime class
   * builder class
   * specialized binding behavior that the builder will own
5. For accumulators, also show:

   * accumulator class name
   * state fields
   * lifecycle members
6. New base class
   
   * If willing to create a new base class, request approval from user.
6. Show target file decision:

   * selected file
   * new file named `function`
   * new file named `accumulator`
7. Show code preview.
8. Show the conformance anchor preview.
9. Ask for explicit confirmation:

```text
Confirm implement? (yes/no)
```

Only continue on explicit confirmation.

## Edit Rules

When confirmed:

* Apply only the minimum edits needed to add the generated class.
* Preserve the file's indentation and style.
* Do not create commits.
* Do not stage files.

For syntax-driven functions:

* keep runtime evaluation semantics in the runtime `IFunction`;
* keep parser-to-runtime binding semantics in the dedicated builder;
* modify parser behavior only when required by the operator's syntax;
* do not refactor unrelated factory code as part of implementing one operator.

For accumulators:

* implement under the existing accumulator architecture;
* preserve the `Initialize` / `Accumulate` / `GetValue` lifecycle;
* do not modify `AccumulatorFactory` unless parameterized accumulator construction is explicitly part of the requested change.

## Final Response Checklist

After successful update, report:

* Source JSON file used.
* Target implementation file updated or created.
* Implemented class name.
* Operator kind.
* Implementation mode.
* Constructors generated, including defaulted overloads if any.
* Mandatory override scaffold added with `TODO REVIEW Scaffold`, where applicable.
* For accumulators:

  * accumulator lifecycle members generated;
  * accumulator conformance anchor file created or updated.
* For syntax-driven functions:

  * builder class created or updated;
  * specialized binding responsibilities implemented;
  * focused builder tests created or updated when required.
* Conformance test anchor file created or updated in `Expressif.Testing`.
* Conformance anchor methods generated from YAML test IDs.
* Explicitly state that no commit was created.
