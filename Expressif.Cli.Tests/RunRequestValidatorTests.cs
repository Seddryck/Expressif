using Expressif.Cli.Application;

namespace Expressif.Cli.Tests;

public class RunRequestValidatorTests
{
    [TestCase(false, false, false, (int)RunInputMode.None)]
    [TestCase(true, false, false, (int)RunInputMode.Repeated)]
    [TestCase(false, true, false, (int)RunInputMode.Batch)]
    [TestCase(true, true, false, (int)RunInputMode.RepeatedAndBatch)]
    [TestCase(false, false, true, (int)RunInputMode.Source)]
    [TestCase(true, false, true, (int)RunInputMode.Conflicting)]
    [TestCase(false, true, true, (int)RunInputMode.Conflicting)]
    public void ResolveInputMode_MapsInputCombination(
        bool hasInput,
        bool hasBatch,
        bool hasSource,
        int expected)
        => Assert.That(
            RunRequestValidator.ResolveInputMode(CreateRequest(hasInput, hasBatch, hasSource)),
            Is.EqualTo((RunInputMode)expected));

    [Test]
    public void Validate_RepeatedAndBatchInput_IsAllowed()
        => Assert.That(
            RunRequestValidator.Validate(CreateRequest(hasInput: true, hasBatch: true)),
            Is.Null);

    [Test]
    public void Validate_ScalarConflictingSource_ReportsSourceConflict()
    {
        var request = CreateRequest(hasInput: true, hasSource: true) with { Scalar = true };

        Assert.That(
            RunRequestValidator.Validate(request),
            Is.EqualTo("The --source option cannot be combined with --input or --batch."));
    }

    private static RunRequest CreateRequest(
        bool hasInput = false,
        bool hasBatch = false,
        bool hasSource = false)
        => new(
            InlineExpression: "absolute",
            ExpressionFilePath: null,
            InputRows: [],
            BatchInput: null,
            SourcePath: null,
            SourceOptions: [],
            Scalar: false,
            HasInput: hasInput,
            HasBatch: hasBatch,
            HasSource: hasSource,
            HasSourceOptions: false,
            BatchOccurrences: hasBatch ? 1 : 0);
}
