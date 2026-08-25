namespace Expressif.Cli.Application;

internal static class RunRequestValidator
{
    public static string? Validate(RunRequest request)
    {
        if (request.Scalar && !request.HasSource)
            return "The --scalar option requires --source.";
        if (request.BatchOccurrences > 1)
            return "The --batch option can only be specified once.";
        if (!request.HasInput && !request.HasBatch && !request.HasSource)
            return "The run command requires inputs. Provide --input, --batch, or --source.";
        if (request.HasSource && (request.HasInput || request.HasBatch))
            return "The --source option cannot be combined with --input or --batch.";
        if (request.HasSourceOptions && !request.HasSource)
            return "The --source-option option requires --source.";
        return null;
    }
}
