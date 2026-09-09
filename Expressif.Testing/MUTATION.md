# Mutation testing

Stryker.NET mutates the Expressif implementation and runs the existing NUnit
conformance cases against it. Behavioral gaps found through surviving mutants
belong in `conformance/`; the YAML format does not need mutation-specific fields.

From the repository root, restore the pinned tools and run the focused pilot:

```powershell
dotnet tool restore
./Mutation.ps1 -AddOnly
```

The pilot selects the `Add` class in `ArithmeticFunctions.cs` and only its
conformance tests. The runner calculates the class's character span at runtime,
so line-ending differences and edits above the class do not shift the scope.
It fails if the class cannot be found. Temporary configuration is removed after
the run. The standard Stryker mutation level is used.

To run the arithmetic source file against all conformance tests:

```powershell
./Mutation.ps1
```

Edit `stryker-config.json` to expand the production source scope. The target is
explicitly `net10.0` because the projects target multiple frameworks.
The runner temporarily sets `ExpressifMutation=true`, enabling literal framework
settings in the source and test projects to work around
[Stryker issue 3758](https://github.com/stryker-mutator/stryker-net/issues/3758).
Normal builds retain the frameworks inherited from `Directory.Build.props`.
The HTML and JSON reports are in `Expressif.Testing/StrykerOutput/<run>/reports/`.
Generated reports and temporary builds are ignored by Git.

## Interpreting results

- **Killed:** a test detected the changed implementation.
- **Survived:** review whether a meaningful behavioral case is missing or the
  mutation is equivalent for the specified behavior.
- **No coverage:** the selected tests did not execute the mutated code.
- **Timeout / compile error:** inspect separately from assertion-detected defects.

Add ordinary conformance cases for meaningful gaps, confirm they pass against
the original implementation, and rerun Stryker. Do not invent behavior or add
exclusions just to increase the score. A focused score covers only the selected
source and generated mutations, not the complete operator contract or runtime.
For .NET-specific behavior, use ordinary unit tests and a separate configuration
without the conformance filter.

The initial score failure threshold is zero while establishing a baseline;
tool/build/test failures still fail the runner. The manual **Mutation testing**
GitHub Actions workflow runs the add pilot and uploads reports. It is separate
from release and conformance packaging jobs. Once merged, dispatch it through
the Actions UI. Raise the score threshold after reviewing a representative baseline.

## Initial add pilot

On 2026-09-09, Stryker 4.16.0 ran the 12 existing add conformance cases and
tested two mutations in `Add.EvaluateNumeric`:

| Mutation | Outcome |
| --- | --- |
| Replace addition with subtraction | Killed |
| Replace multiplication by the repetition count with division | Killed |

The focused score was 100%, with no survivors, uncovered mutations, timeouts,
or compile errors inside Add. No additional conformance cases were needed for
these two mutations. The run took about 29 seconds on the development machine.
Stryker also logged compile-error rollbacks in unrelated code while preparing
the assembly; these were outside the selected Add scope. This result does not
establish a mutation score for that other code or for shared numeric coercion.
