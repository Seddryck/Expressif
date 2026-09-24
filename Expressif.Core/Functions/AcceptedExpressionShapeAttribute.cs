namespace Expressif.Functions;

/// <summary>Describes the bound expression shape required by a constructor parameter.</summary>
internal enum AcceptedExpressionShape
{
    OpenExpression,
    DirectFieldSelector,
    CallableReference,
}

/// <summary>Declares a required expression shape, independent of argument role and evaluation context.</summary>
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class AcceptedExpressionShapeAttribute(AcceptedExpressionShape shape) : Attribute
{
    public AcceptedExpressionShape Shape { get; } = shape;
}
