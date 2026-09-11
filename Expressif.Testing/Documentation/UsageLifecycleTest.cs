using Expressif.Semantics;
using Expressif.Syntax;

namespace Expressif.Testing.Documentation;

public class UsageLifecycleTest
{
    [Test]
    public void ActiveRules_HaveStableIdentityAndSeparateLifecycleFields()
    {
        Assert.That(UsageLifecycle.All.Select(rule => rule.Id), Is.EquivalentTo(new[]
        {
            "implicit-tuple-binding-adjacent", "implicit-tuple-binding-chunk-while",
            "implicit-tuple-binding-map-over", "implicit-tuple-binding-map-with",
        }));
        foreach (var rule in UsageLifecycle.All)
        {
            Assert.Multiple(() =>
            {
                Assert.That(rule.Active, Is.True);
                Assert.That(rule.Sunset, Is.EqualTo("3.0"));
                Assert.That(rule.DeprecatedSince is null || Version.TryParse(rule.DeprecatedSince, out _), Is.True);
                Assert.That(rule.IntroducedCommit, Does.Match("^[a-f0-9]{40}$"));
                Assert.That(rule.Condition, Is.Not.Empty);
                Assert.That(rule.MigrationConditions, Is.Not.Empty);
                Assert.That(rule.Examples, Is.Not.Empty);
            });
        }
    }

    public static IEnumerable<TestCaseData> Migrations()
        => UsageLifecycle.All.SelectMany(rule => rule.Examples
            .Where(example => example.Availability == "supported")
            .Select(example => new TestCaseData(rule, example).SetName($"Migration: {example.Replacement}")));

    [TestCaseSource(nameof(Migrations))]
    public void DocumentedMigration_MatchesResolvedDiagnosticAndPreservesResult(UsageLifecycleRule rule, UsageMigrationExample example)
    {
        var analyzer = new LegacyTupleBindingAnalyzer();
        var use = analyzer.Analyze(ExpressionParser.Parse(example.Deprecated)).Single();
        var result = Expression.Create(example.Expected).Evaluate(null);
        Assert.Multiple(() =>
        {
            Assert.That(use.Lifecycle, Is.SameAs(rule));
            Assert.That(use.RuleId, Is.EqualTo(rule.Id));
            Assert.That(use.Code, Is.EqualTo(rule.DiagnosticCode));
            Assert.That(use.CanRewrite, Is.True);
            Assert.That(use.Replacement, Is.EqualTo(rule.ReplacementFor(use.Callable)));
            Assert.That(analyzer.Analyze(ExpressionParser.Parse(example.Replacement)), Is.Empty);
            Assert.That(Expression.Create(example.Deprecated).Evaluate(null), Is.EqualTo(result));
            Assert.That(Expression.Create(example.Replacement).Evaluate(null), Is.EqualTo(result));
        });
    }

    [Test]
    public void ChunkWhile_FollowingArgumentsRetainPreviousCurrentPair()
    {
        const string legacy = "{1, 2, 5} | chunk-while(subtract | less-than($1))";
        const string replacement = "{1, 2, 5} | chunk-while($1 | subtract($0) | less-than($1))";
        Assert.That(Expression.Create(replacement).Evaluate(null), Is.EqualTo(Expression.Create(legacy).Evaluate(null)));
    }
}
