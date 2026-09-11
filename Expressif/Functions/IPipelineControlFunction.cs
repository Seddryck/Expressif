namespace Expressif.Functions;

internal interface IPipelineControlFunction : IFunction
{
    object? Evaluate(object? value, out bool terminate);
}
