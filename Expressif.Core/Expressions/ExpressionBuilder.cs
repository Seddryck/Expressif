using Expressif.Bindings;
using Expressif.Functions;
using Expressif.Serialization;

namespace Expressif;

/// <summary>Starts a typed, persistent expression pipeline.</summary>
public sealed class ExpressionBuilder
{
    private readonly FunctionFactory factory;

    internal ExpressionBuilder(FunctionFactory factory)
        => this.factory = factory ?? throw new ArgumentNullException(nameof(factory));

    public Pipeline Create<T>()
        where T : IFunction
        => Create(typeof(T));

    public Pipeline Create<T>(params object?[] arguments)
        where T : IFunction
        => Create(typeof(T), arguments);

    public Pipeline Create(Type type, params object?[] arguments)
        => new(factory, [CreateFunction(type, arguments)]);

    private static Bindings.Function CreateFunction(Type type, object?[] arguments)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(arguments);
        if (!typeof(IFunction).IsAssignableFrom(type))
        {
            throw new ArgumentException(
                $"The type '{type.FullName}' does not implement '{nameof(IFunction)}'.",
                nameof(type));
        }

        return Bindings.Function.FromParameters(type.Name, Parametrize(arguments));
    }

    private static IReadOnlyList<IParameter> Parametrize(IEnumerable<object?> arguments)
        => BindingCollections.Freeze(arguments.Select(argument => argument switch
        {
            IParameter parameter => parameter,
            _ => new LiteralParameter(argument),
        }));

    /// <summary>Represents a non-empty immutable expression pipeline.</summary>
    public sealed class Pipeline
    {
        private readonly FunctionFactory factory;
        private readonly IReadOnlyList<Bindings.Function> stages;

        internal Pipeline(FunctionFactory factory, IEnumerable<Bindings.Function> stages)
            => (this.factory, this.stages) = (factory, BindingCollections.Freeze(stages));

        public Pipeline Then<T>()
            where T : IFunction
            => Then(typeof(T));

        public Pipeline Then<T>(params object?[] arguments)
            where T : IFunction
            => Then(typeof(T), arguments);

        public Pipeline Then(Type type, params object?[] arguments)
            => new(factory, stages.Append(CreateFunction(type, arguments)));

        public Pipeline Then(Pipeline pipeline)
        {
            ArgumentNullException.ThrowIfNull(pipeline);
            if (!ReferenceEquals(factory, pipeline.factory))
            {
                throw new ArgumentException("Pipelines must originate from the same Expressif environment.", nameof(pipeline));
            }

            return new(factory, stages.Concat(pipeline.stages));
        }

        public IExpression Build()
            => new Expression(factory.Instantiate(new OpenRootExpression(new OpenExpression(stages))));

        public string ToSource() => new ExpressionSerializer().Serialize(new OpenExpression(stages));
    }
}
