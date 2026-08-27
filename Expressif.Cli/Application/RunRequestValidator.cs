namespace Expressif.Cli.Application;

internal enum RunInputMode
{
    None,
    Repeated,
    Batch,
    RepeatedAndBatch,
    Source,
    Conflicting,
}

internal static class RunRequestValidator
{
    public static string? Validate(RunRequest request)
    {
        var mode = ResolveInputMode(request);
        return ValidateScalar(request)
            ?? ValidateBatchOccurrences(request)
            ?? ValidateInputMode(mode)
            ?? ValidateSourceOptions(request, mode);
    }

    internal static RunInputMode ResolveInputMode(RunRequest request)
        => (request.HasInput, request.HasBatch, request.HasSource) switch
        {
            (_, _, true) when request.HasInput || request.HasBatch => RunInputMode.Conflicting,
            (_, _, true) => RunInputMode.Source,
            (true, true, false) => RunInputMode.RepeatedAndBatch,
            (true, false, false) => RunInputMode.Repeated,
            (false, true, false) => RunInputMode.Batch,
            _ => RunInputMode.None,
        };

    private static string? ValidateScalar(RunRequest request)
        => request.Scalar && !request.HasSource
            ? "The --scalar option requires --source."
            : null;

    private static string? ValidateBatchOccurrences(RunRequest request)
        => request.BatchOccurrences > 1
            ? "The --batch option can only be specified once."
            : null;

    private static string? ValidateInputMode(RunInputMode mode)
        => mode switch
        {
            RunInputMode.None => "The run command requires inputs. Provide --input, --batch, or --source.",
            RunInputMode.Conflicting => "The --source option cannot be combined with --input or --batch.",
            _ => null,
        };

    private static string? ValidateSourceOptions(RunRequest request, RunInputMode mode)
        => request.HasSourceOptions && mode != RunInputMode.Source
            ? "The --source-option option requires --source."
            : null;
}
