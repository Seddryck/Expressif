using System.Text.Json;
using Expressif.Introspection;
using Expressif.Library.Catalog;
using Expressif.Discovery;
using Expressif.Functions;

namespace Expressif.Testing.Catalog;

[TestFixture]
public class FunctionCatalogTest
{
    [Test]
    [Category("MetadataConsistency")]
    public void Default_ExpressifAssembly_ContainsEmbeddedCatalog()
    {
        var assembly = typeof(FunctionCatalog).Assembly;

        Assert.Multiple(() =>
        {
            Assert.That(assembly.GetManifestResourceNames(), Does.Contain(FunctionCatalog.ResourceName));
            Assert.That(assembly.GetManifestResourceNames(), Does.Contain(FunctionCatalog.PredicateResourceName));
        });
        Assert.That(FunctionCatalog.Default.Functions, Is.Not.Empty);
    }

    [Test]
    [Category("MetadataConsistency")]
    public void Default_PublicFunctions_MatchIntrospectionMetadata()
    {
        var documented = FunctionCatalog.Default.Functions
            .Where(x => x.Kind != "predicate")
            .ToDictionary(x => x.Name);
        var introspected = ExpressifIntrospection.Functions.Describe().Where(x => x.IsPublic).ToDictionary(x => x.Name);

        Assert.That(documented.Keys, Is.EquivalentTo(introspected.Keys));
        foreach (var (name, implementation) in introspected)
        {
            var documentation = documented[name];
            using (Assert.EnterMultipleScope())
            {
                Assert.That(documentation.Aliases, Is.EquivalentTo(implementation.Aliases), $"Aliases for {name}");
                Assert.That(documentation.Scope, Is.EqualTo(implementation.Scope), $"Scope for {name}");
                Assert.That(documentation.Input, Is.EqualTo(implementation.Input), $"Input for {name}");
                Assert.That(documentation.Output, Is.EqualTo(implementation.Output), $"Output for {name}");
                Assert.That(documentation.Summary, Is.Not.Empty, $"Summary for {name}");
                Assert.That(documentation.Deprecated, Is.EqualTo(implementation.Deprecated), $"Deprecation for {name}");
                Assert.That(documentation.Replacement, Is.EqualTo(implementation.Replacement), $"Replacement for {name}");
                Assert.That(documentation.Sunset, Is.EqualTo(implementation.Sunset), $"Sunset for {name}");
                Assert.That(documentation.ReplacementIsEquivalent, Is.EqualTo(implementation.ReplacementIsEquivalent),
                    $"Replacement equivalence for {name}");
                Assert.That(documentation.MigrationNotes, Is.EqualTo(implementation.MigrationNotes),
                    $"Migration notes for {name}");
                Assert.That(documentation.Parameters.Select(x => (x.Name, Type: x.TypeOrKind, x.Optional, x.Variadic, x.AllowsSpread)),
                    Is.EqualTo(implementation.Parameters.Select(x => (x.Name, x.Type, x.Optional, x.Variadic, x.AllowsSpread))),
                    $"Parameters for {name}");
                Assert.That(documentation.Parameters.Select(x => x.Summary), Is.All.Not.Empty, $"Parameter summaries for {name}");
            }
        }
    }

    [Test]
    [Category("MetadataConsistency")]
    public void Default_LifecycleMetadata_IsConsistent()
    {
        foreach (var function in FunctionCatalog.Default.Functions)
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(function.Replacement is null || function.Deprecated, Is.True,
                    $"Replacement for {function.Name} requires deprecation.");
                Assert.That(function.Sunset is null || function.Deprecated, Is.True,
                    $"Sunset for {function.Name} requires deprecation.");
                Assert.That(!function.ReplacementIsEquivalent || function.Replacement is not null, Is.True,
                    $"Equivalent replacement for {function.Name} requires a replacement.");
                Assert.That(function.MigrationNotes is null || function.Replacement is not null, Is.True,
                    $"Migration notes for {function.Name} require a replacement.");
                Assert.That(function.Replacement is null || FunctionCatalog.Default.Find(function.Replacement)?.IsPublic == true,
                    Is.True, $"Replacement for {function.Name} must resolve to a public callable.");
            }
        }
    }

    [TestCase("append", "suffix", "3.0")]
    [TestCase("prepend", "prefix", "3.0")]
    [TestCase("append-space", "suffix-space", "3.0")]
    [TestCase("append-new-line", "suffix-new-line", "3.0")]
    [TestCase("prepend-space", "prefix-space", "3.0")]
    [TestCase("prepend-new-line", "prefix-new-line", "3.0")]
    public void Default_DeprecatedTextFunction_ExposesLifecycle(
        string name,
        string replacement,
        string sunset)
    {
        var function = FunctionCatalog.Default.Find(name);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(function?.Deprecated, Is.True);
            Assert.That(function?.Replacement, Is.EqualTo(replacement));
            Assert.That(function?.Sunset, Is.EqualTo(sunset));
            Assert.That(function?.ReplacementIsEquivalent, Is.False);
            Assert.That(function?.MigrationNotes, Does.Contain("preserves null input"));
        }
    }

    [Test]
    public void LifecycleAttribute_Values_ExposesDeprecationMetadata()
    {
        var lifecycle = new FunctionLifecycleAttribute(
            "replacement", "3.0", replacementIsEquivalent: true, migrationNotes: "No behavior change.");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(lifecycle.Deprecated, Is.True);
            Assert.That(lifecycle.Replacement, Is.EqualTo("replacement"));
            Assert.That(lifecycle.Sunset, Is.EqualTo("3.0"));
            Assert.That(lifecycle.ReplacementIsEquivalent, Is.True);
            Assert.That(lifecycle.MigrationNotes, Is.EqualTo("No behavior change."));
        }
    }

    [Test]
    public void LifecycleRecords_Values_ExposeDeprecationMetadata()
    {
        var documentation = new FunctionDocumentation(
            "sample", true, [], "special", "any", "any", "Summary.", [],
            Deprecated: true, Replacement: "replacement", Sunset: "3.0",
            ReplacementIsEquivalent: true, MigrationNotes: "No behavior change.");
        var implementation = new FunctionInfo(new FunctionInfoDefinition
        {
            Name = "sample",
            IsPublic = true,
            Aliases = [],
            Scope = "special",
            Input = "any",
            Output = "any",
            Converted = false,
            Reason = "Reason.",
            ImplementationType = typeof(object),
            Summary = "Summary.",
            Parameters = [],
            Deprecated = true,
            Replacement = "replacement",
            Sunset = "3.0",
            ReplacementIsEquivalent = true,
            MigrationNotes = "No behavior change.",
            Signatures = [],
        });

        using (Assert.EnterMultipleScope())
        {
            Assert.That(documentation.Deprecated, Is.True);
            Assert.That(documentation.Replacement, Is.EqualTo("replacement"));
            Assert.That(documentation.Sunset, Is.EqualTo("3.0"));
            Assert.That(documentation.ReplacementIsEquivalent, Is.True);
            Assert.That(documentation.MigrationNotes, Is.EqualTo("No behavior change."));
            Assert.That(implementation.Deprecated, Is.True);
            Assert.That(implementation.Replacement, Is.EqualTo("replacement"));
            Assert.That(implementation.Sunset, Is.EqualTo("3.0"));
            Assert.That(implementation.ReplacementIsEquivalent, Is.True);
            Assert.That(implementation.MigrationNotes, Is.EqualTo("No behavior change."));
        }
    }

    [Test]
    public void Find_Alias_ReturnsCanonicalFunction()
        => Assert.That(FunctionCatalog.Default.Find("array-to-broadcast")?.Name, Is.EqualTo("broadcast"));

    [Test]
    public void Find_PredicateAlias_ReturnsCanonicalPredicateMetadata()
    {
        var predicate = FunctionCatalog.Default.Find("greater-than");

        Assert.Multiple(() =>
        {
            Assert.That(predicate?.Name, Is.EqualTo("is-greater-than"));
            Assert.That(predicate?.Kind, Is.EqualTo("predicate"));
            Assert.That(predicate?.Input, Is.EqualTo("numeric"));
            Assert.That(predicate?.Output, Is.EqualTo("boolean"));
            Assert.That(predicate?.Parameters,
                Has.Exactly(1).Matches<FunctionParameterDocumentation>(parameter =>
                    parameter.Name == "reference"
                    && parameter.Type == "numeric"
                    && !parameter.Optional));
        });
    }

    [TestCase("first", "function", "first-elements")]
    [TestCase("last", "function", "last-elements")]
    [TestCase("first", "accumulator", "first")]
    [TestCase("last", "accumulator", "last")]
    public void Find_KindConstraint_ResolvesCrossKindName(
        string name,
        string kind,
        string canonical)
        => Assert.That(FunctionCatalog.Default.Find(name, kind)?.Name, Is.EqualTo(canonical));

    [TestCase("first")]
    [TestCase("last")]
    public void Find_WithoutKind_ReturnsNullForCrossKindAmbiguity(string name)
        => Assert.That(FunctionCatalog.Default.Find(name), Is.Null);

    [Test]
    public void Merge_ExplicitFunctionKindsRemainUnchangedAndPredicatesAreMarked()
    {
        var function = Documentation("sum");
        var predicate = Documentation("is-positive");
        var accumulator = Documentation("first") with { Kind = "accumulator", Input = "any", Output = "any" };

        var merged = FunctionCatalog.Merge([function, accumulator], [predicate]);

        Assert.Multiple(() =>
        {
            Assert.That(merged.Single(entry => entry.Name == "sum").Kind, Is.EqualTo("function"));
            Assert.That(merged.Single(entry => entry.Name == "is-positive").Kind, Is.EqualTo("predicate"));
            Assert.That(merged.Single(entry => entry.Name == "first").Kind, Is.EqualTo("accumulator"));
            Assert.That(merged.Single(entry => entry.Name == "first").Input, Is.EqualTo("any"));
            Assert.That(merged.Single(entry => entry.Name == "first").Output, Is.EqualTo("any"));
        });
    }

    [TestCase("canonical")]
    [TestCase("alias")]
    [TestCase("deprecated-alias")]
    public void ValidateNames_CollisionWithinKind_ThrowsClearDiagnostic(string collisionKind)
    {
        var first = collisionKind switch
        {
            "canonical" => Documentation("shared"),
            "alias" => Documentation("first", ["shared"]),
            "deprecated-alias" => Documentation("first") with
            {
                DeprecatedAliases = [new("shared", "first", "Use first.")],
            },
            _ => throw new ArgumentOutOfRangeException(nameof(collisionKind)),
        };
        var second = Documentation(collisionKind == "canonical" ? "shared" : "second", ["shared"])
            with { Scope = "other" };
        var expected = collisionKind == "canonical"
            ? "Catalog name 'shared' is ambiguous between function 'shared', function 'shared'."
            : "Catalog name 'shared' is ambiguous between function 'first', function 'second'.";

        Assert.That(
            () => FunctionCatalog.ValidateNames([first, second]),
            Throws.InvalidOperationException.With.Message.EqualTo(expected));
    }

    [Test]
    public void ValidateNames_CollisionAcrossKinds_IsAllowed()
        => Assert.That(
            () => FunctionCatalog.ValidateNames([
                Documentation("shared"),
                Documentation("shared") with { Kind = "accumulator" },
            ]),
            Throws.Nothing);

    [TestCase("array", "values", 0)]
    [TestCase("record", "entries", 0)]
    [TestCase("coalesce", "expressions", 2)]
    [TestCase("transform-with", "expressions", 1)]
    [TestCase("transform-as", "expressions", 1)]
    public void Default_VariadicParameter_DeserializesMinimumCardinality(
        string function,
        string parameter,
        int minimumCardinality)
        => Assert.That(
            FunctionCatalog.Default.Find(function)?.Parameters.Single(x => x.Name == parameter).MinimumCardinality,
            Is.EqualTo(minimumCardinality));

    [TestCase("array", "values", true)]
    [TestCase("nested-field", "path", true)]
    [TestCase("sort-key", "values", true)]
    [TestCase("record", "entries", false)]
    [TestCase("coalesce", "expressions", false)]
    public void Default_VariadicParameter_DeclaresPositionalSpreadSeparately(
        string function, string parameter, bool allowsSpread)
        => Assert.That(
            FunctionCatalog.Default.Find(function)?.Parameters.Single(x => x.Name == parameter).AllowsSpread,
            Is.EqualTo(allowsSpread));

    [Test]
    public void Default_RuntimeOmissionContracts_AgreeWithCatalog()
    {
        foreach (var implementation in ExpressifIntrospection.Functions.Describe().Where(item => item.IsPublic))
        {
            var documentation = FunctionCatalog.Default.Find(implementation.Name)!;
            var annotated = implementation.ImplementationType.GetConstructors()
                .SelectMany(constructor => constructor.GetParameters())
                .Where(parameter => parameter.IsDefined(typeof(ArgumentOmissionAttribute), false));
            foreach (var parameter in annotated)
            {
                var runtime = parameter.GetCustomAttributes(typeof(ArgumentOmissionAttribute), false)
                    .Cast<ArgumentOmissionAttribute>().Single();
                var documented = documentation.Parameters.Single(item =>
                    item.Name == parameter.Name!.ToKebabCase());
                var expected = runtime.Mode switch
                {
                    ArgumentOmissionMode.EmptyVariadic => ParameterOmissionMode.EmptyVariadic,
                    ArgumentOmissionMode.Absent => ParameterOmissionMode.Absent,
                    _ => throw new InvalidOperationException($"Unsupported runtime omission mode '{runtime.Mode}'."),
                };
                Assert.That(documented.Omission?.Mode, Is.EqualTo(expected),
                    $"Omission for {implementation.Name}.{documented.Name}");
            }
        }
    }

    [Test]
    public void Find_CaseVariantAliasForSameFunction_ReturnsCanonicalFunction()
        => Assert.That(FunctionCatalog.Default.Find("FILE-TO-CREATION-DATETIME")?.Name, Is.EqualTo("creation-datetime"));

    [Test]
    public void ForScope_CaseInsensitive_ReturnsOnlyRequestedScope()
        => Assert.That(FunctionCatalog.Default.ForScope("record").Select(x => x.Scope), Is.All.EqualTo("record"));

    [Test]
    public void Suggest_CloseName_ReturnsExpectedFunctionFirst()
        => Assert.That(FunctionCatalog.Default.Suggest("revers").First().Name, Is.EqualTo("reverse"));

    [Test]
    public void Suggest_ClosePredicateAlias_ReturnsExpectedPredicateFirst()
        => Assert.That(FunctionCatalog.Default.Suggest("greter-than").First().Name, Is.EqualTo("is-greater-than"));

    [Test]
    public void Suggest_KindConstraint_ReturnsAccumulator()
        => Assert.That(FunctionCatalog.Default.Suggest("frist", "accumulator").First().Name, Is.EqualTo("first"));

    [Test]
    public void Default_FunctionWithExamples_DeserializesExamples()
        => Assert.That(
            FunctionCatalog.Default.Find("add")?.Examples,
            Is.EqualTo(new[] { "10 | add(5)      → 15", "10 | add(5, 2)   → 20" }));

    [TestCase("subtract", "times", "integer", true)]
    [TestCase("power", "exponent", "numeric", false)]
    [TestCase("nth-root", "exponent", "numeric", false)]
    [TestCase("swap", "first", "integer", true)]
    [TestCase("swap", "second", "integer", true)]
    [TestCase("change-of-hour", "hour", "integer", false)]
    [TestCase("change-of-minute", "minute", "integer", false)]
    [TestCase("change-of-second", "second", "integer", false)]
    [TestCase("change-of-month", "month", "integer", false)]
    [TestCase("change-of-year", "year", "integer", false)]
    [TestCase("local-to-utc", "timeZoneLabel", "text", false)]
    [TestCase("utc-to-local", "timeZoneLabel", "text", false)]
    [TestCase("catholic-calendar", "event", "text", false)]
    [TestCase("catholic-calendar", "kind", "text", true)]
    public void Default_CorrectedRuntimeParameter_HasCanonicalMetadata(
        string function,
        string parameter,
        string type,
        bool optional)
    {
        var metadata = FunctionCatalog.Default.Find(function)?.Parameters.Single(x => x.Name == parameter);

        Assert.Multiple(() =>
        {
            Assert.That(metadata?.Type, Is.EqualTo(type));
            Assert.That(metadata?.Optional, Is.EqualTo(optional));
            Assert.That(metadata?.Evaluation,
                Is.EqualTo(new ParameterEvaluationDocumentation(
                    "once",
                    $"Evaluated once in the context surrounding this `{function}` call.",
                    Source: "enclosing")));
        });
    }

    [TestCase("subtract", "times", "1")]
    [TestCase("catholic-calendar", "kind", "\"Local\"")]
    public void Default_OptionalRuntimeParameter_HasConstantDefault(
        string function,
        string parameter,
        string value)
    {
        var omission = FunctionCatalog.Default.Find(function)?.Parameters
            .Single(x => x.Name == parameter).Omission;

        Assert.Multiple(() =>
        {
            Assert.That(omission?.Mode, Is.EqualTo(ParameterOmissionMode.Constant));
            Assert.That(omission?.Value.GetRawText(), Is.EqualTo(value));
        });
    }

    [Test]
    public void Default_FunctionWithBehavior_DeserializesBehavior()
        => Assert.That(
            FunctionCatalog.Default.Find("adjacent")?.Behavior,
            Does.StartWith("The operation receives T(previous, current)."));

    [TestCase("map", "preserved", "per-element", "preserved")]
    [TestCase("filter", "non-increasing", "per-element", "preserved")]
    [TestCase("fold", "collapsed", "whole-input", "not-applicable")]
    [TestCase("broadcast", "preserved", "whole-input", "preserved")]
    [TestCase("scan", "preserved", "prefix", "preserved")]
    [TestCase("group-by", "partitioned", "partition", "preserved")]
    public void Default_StructuralFunction_DeserializesSemantics(
        string function,
        string cardinality,
        string dependency,
        string ordering)
    {
        var semantics = FunctionCatalog.Default.Find(function)?.Semantics;

        Assert.That(semantics, Is.EqualTo(new FunctionSemanticsDocumentation(cardinality, dependency, ordering)));
    }

    [Test]
    public void Default_Map_DeserializesSchemaContract()
    {
        var schema = FunctionCatalog.Default.Find("map")?.Schema;

        Assert.Multiple(() =>
        {
            Assert.That(schema?.Input, Is.EqualTo("array<T>"));
            Assert.That(schema?.Output, Is.EqualTo("array<U>"));
            Assert.That(schema?.Parameters?["transformation"],
                Is.EqualTo(new FunctionParameterSchemaDocumentation("T", "U")));
            Assert.That(schema?.Classification, Is.EqualTo("contract"));
            Assert.That(schema?.NullableWhen, Is.EqualTo(new[] { "input" }));
            Assert.That(FunctionCatalog.Default.Find("field")?.Schema?.Intrinsic, Is.EqualTo("field"));
            Assert.That(FunctionCatalog.Default.Find("field")?.Schema?.Classification, Is.EqualTo("intrinsic"));
        });
    }

    [Test]
    public void Default_RepresentativeContracts_DeserializeCompositionMetadata()
    {
        var coalesce = FunctionCatalog.Default.Find("coalesce")?.Schema;
        var groupBy = FunctionCatalog.Default.Find("group-by")?.Schema;
        var sum = FunctionCatalog.Default.Find("sum", "accumulator")?.Schema;

        Assert.Multiple(() =>
        {
            Assert.That(coalesce?.Parameters?["expressions"].Combine, Is.EqualTo("union"));
            Assert.That(groupBy?.Output, Is.EqualTo("grouping<K, T>"));
            Assert.That(groupBy?.Parameters?["expressions"].Combine, Is.EqualTo("tuple"));
            Assert.That(sum, Is.EqualTo(new FunctionSchemaDocumentation(
                "numeric",
                "numeric",
                Classification: "contract")));
        });
    }

    [Test]
    public void Default_AccumulatorContracts_ReflectImplementationRelationships()
    {
        var any = FunctionCatalog.Default.Find("any", "accumulator")!;
        var closest = FunctionCatalog.Default.Find("closest", "accumulator")!.Schema!;
        var every = FunctionCatalog.Default.Find("every", "accumulator")!;
        var last = FunctionCatalog.Default.Find("last", "accumulator")!.Schema!;
        var maximum = FunctionCatalog.Default.Find("max", "accumulator")!.Schema!;
        var minimum = FunctionCatalog.Default.Find("min", "accumulator")!.Schema!;
        var only = FunctionCatalog.Default.Find("only", "accumulator")!.Schema!;
        var reduce = FunctionCatalog.Default.Find("reduce", "accumulator")!.Schema!;

        Assert.Multiple(() =>
        {
            Assert.That((any.Input, any.Output, any.Schema!.Classification),
                Is.EqualTo(("boolean", "boolean", "fixed")));
            Assert.That((every.Input, every.Output, every.Schema!.Classification),
                Is.EqualTo(("boolean", "boolean", "fixed")));
            Assert.That((closest.Classification, closest.Input, closest.Output),
                Is.EqualTo(("contract", "T", "nullable<T>")));
            Assert.That((last.Classification, last.Input, last.Output),
                Is.EqualTo(("contract", "T", "nullable<T>")));
            Assert.That((maximum.Classification, maximum.Input, maximum.Output),
                Is.EqualTo(("contract", "numeric", "nullable<numeric>")));
            Assert.That((minimum.Classification, minimum.Input, minimum.Output),
                Is.EqualTo(("contract", "numeric", "nullable<numeric>")));
            Assert.That((only.Classification, only.Input, only.Output),
                Is.EqualTo(("contract", "T", "U")));
            Assert.That(only.Parameters!["predicate"],
                Is.EqualTo(new FunctionParameterSchemaDocumentation("T", "boolean")));
            Assert.That(only.Parameters["accumulator"],
                Is.EqualTo(new FunctionParameterSchemaDocumentation("T", "U")));
            Assert.That(reduce.Classification, Is.EqualTo("dynamic"));
            Assert.That(reduce.DynamicReason, Does.Contain("operation output"));
            Assert.That(reduce.DynamicReason, Does.Contain("initial value"));
        });
    }

    [Test]
    public void Default_AllPublicCallables_HaveExactlyOneSchemaClassification()
    {
        var functions = FunctionCatalog.Default.Functions;

        Assert.Multiple(() =>
        {
            Assert.That(functions, Has.All.Property(nameof(FunctionDocumentation.Schema)).Not.Null);
            Assert.That(functions.Select(function => function.Schema!.Classification),
                Is.All.AnyOf("fixed", "contract", "intrinsic", "dynamic"));
            Assert.That(functions.Where(function => function.Schema!.Classification == "dynamic")
                .Select(function => function.Schema!.DynamicReason), Is.All.Not.Empty);
            Assert.That(functions.Where(function => function.Schema!.Classification == "contract")
                .Select(function => function.Schema!.Input), Is.All.Not.Empty);
            Assert.That(functions.Where(function => function.Schema!.Classification == "contract")
                .Select(function => function.Schema!.Output), Is.All.Not.Empty);
            Assert.That(functions.Where(function => function.Schema!.Classification == "intrinsic")
                .Select(function => function.Schema!.Intrinsic), Is.All.Not.Empty);
        });
    }

    [Test]
    public void Default_PairDictionaryAndTupleVocabulary_Deserializes()
    {
        var pair = FunctionCatalog.Default.Find("pair")?.Schema;
        var dictionary = FunctionCatalog.Default.Find("dictionary")?.Schema;
        var tuple = FunctionCatalog.Default.Find("to-tuple")?.Schema;
        var zip = FunctionCatalog.Default.Find("zip")?.Schema;

        Assert.Multiple(() =>
        {
            Assert.That(pair?.Output, Is.EqualTo("pair<K, V>"));
            Assert.That(dictionary?.Intrinsic, Is.EqualTo("dictionary"));
            Assert.That(tuple?.Output, Is.EqualTo("variadic-tuple<T>"));
            Assert.That(zip?.NullableWhen, Is.EqualTo(new[] { "input", "array" }));
        });
    }

    [TestCase("add", "times", ParameterOmissionMode.Constant)]
    [TestCase("subtract", "times", ParameterOmissionMode.Constant)]
    [TestCase("swap", "first", ParameterOmissionMode.Absent)]
    [TestCase("swap", "second", ParameterOmissionMode.Absent)]
    [TestCase("catholic-calendar", "kind", ParameterOmissionMode.Constant)]
    [TestCase("array", "values", ParameterOmissionMode.EmptyVariadic)]
    [TestCase("throw", "predicate", ParameterOmissionMode.Absent)]
    [TestCase("distribute-random-split", "seed", ParameterOmissionMode.EnvironmentDerived)]
    public void Default_OptionalParameter_DeserializesOmissionMode(
        string function,
        string parameter,
        ParameterOmissionMode expected)
        => Assert.That(
            FunctionCatalog.Default.Find(function)?.Parameters.Single(x => x.Name == parameter).Omission?.Mode,
            Is.EqualTo(expected));

    [TestCase("{\"Mode\":\"constant\",\"Value\":1}", JsonValueKind.Number, "1")]
    [TestCase("{\"Mode\":\"constant\",\"Value\":\"text\"}", JsonValueKind.String, "\"text\"")]
    [TestCase("{\"Mode\":\"constant\",\"Value\":true}", JsonValueKind.True, "true")]
    [TestCase("{\"Mode\":\"constant\",\"Value\":null}", JsonValueKind.Null, "null")]
    public void ParameterOmission_ConstantValue_RoundTripsWithJsonType(
        string json,
        JsonValueKind expectedKind,
        string expectedValue)
    {
        var omission = JsonSerializer.Deserialize<ParameterOmissionDocumentation>(json)!;
        var serialized = JsonSerializer.Serialize(omission);
        using var document = JsonDocument.Parse(serialized);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(omission.Value.ValueKind, Is.EqualTo(expectedKind));
            Assert.That(document.RootElement.GetProperty("Mode").GetString(), Is.EqualTo("constant"));
            Assert.That(document.RootElement.GetProperty("Value").GetRawText(), Is.EqualTo(expectedValue));
        }
    }

    [TestCase(true, false, "is optional and must declare omission behavior")]
    [TestCase(false, true, "is required and cannot declare omission behavior")]
    public void ValidateOmissions_InconsistentOptionality_Throws(
        bool optional,
        bool hasOmission,
        string message)
    {
        var omission = hasOmission
            ? new ParameterOmissionDocumentation(ParameterOmissionMode.Absent)
            : null;
        var parameter = new FunctionParameterDocumentation("value", "any", optional, "Summary.", Omission: omission);
        var function = new FunctionDocumentation("sample", true, [], "special", "any", "any", "Summary.", [parameter]);

        Assert.That(
            () => FunctionCatalog.ValidateOmissions([function]),
            Throws.InvalidOperationException.With.Message.Contains(message));
    }

    [Test]
    public void ValidateOmissions_NonVariadicParameterCannotAllowSpread()
    {
        var parameter = new FunctionParameterDocumentation("value", "any", false, "Summary.", AllowsSpread: true);
        var function = new FunctionDocumentation("sample", true, [], "special", "any", "any", "Summary.", [parameter]);

        Assert.That(() => FunctionCatalog.ValidateOmissions([function]),
            Throws.InvalidOperationException.With.Message.Contains("allows spread but is not variadic"));
    }

    [TestCase("invalid", "per-element", "preserved", "cardinality")]
    [TestCase("preserved", "invalid", "preserved", "dependency")]
    [TestCase("preserved", "per-element", "invalid", "ordering")]
    public void ValidateSemantics_UnsupportedChoice_Throws(
        string cardinality,
        string dependency,
        string ordering,
        string dimension)
    {
        var semantics = new FunctionSemanticsDocumentation(cardinality, dependency, ordering);
        var function = new FunctionDocumentation(
            "sample", true, [], "special", "any", "any", "Summary.", [], Semantics: semantics);

        Assert.That(
            () => FunctionCatalog.ValidateSemantics([function]),
            Throws.InvalidOperationException.With.Message.Contains($"semantics {dimension}"));
    }

    [TestCase(null, "boolean")]
    [TestCase("text", null)]
    public void ValidateSchemas_FixedSchemaWithoutCanonicalType_Throws(
        string? input,
        string? output)
    {
        var function = Documentation("sample") with
        {
            Input = input!,
            Output = output!,
            Schema = new FunctionSchemaDocumentation(Classification: "fixed"),
        };

        Assert.That(
            () => FunctionCatalog.ValidateSchemas([function]),
            Throws.InvalidOperationException.With.Message.Contains(
                "must declare canonical input and output types"));
    }

    private static FunctionDocumentation Documentation(string name, string[]? aliases = null)
        => new(name, true, aliases ?? [], "special", "any", "any", "Summary.", []);
}
