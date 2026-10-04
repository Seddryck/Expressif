using Expressif.Bindings;
using Expressif.Functions;
using Expressif.Predicates;
using Expressif.Serialization;

namespace Expressif;

/// <summary>Starts a typed, persistent Boolean rule.</summary>
public sealed class PredicationBuilder
{
    private readonly FunctionFactory factory;

    internal PredicationBuilder(FunctionFactory factory)
        => this.factory = factory ?? throw new ArgumentNullException(nameof(factory));

    public Rule Create<T>()
        where T : IPredicate
        => Create(typeof(T));

    public Rule Create<T>(params object?[] arguments)
        where T : IPredicate
        => Create(typeof(T), arguments);

    public Rule Create(Type type, params object?[] arguments)
        => new(factory, CreateLeaf(type, arguments));

    private static IPredication CreateLeaf(Type type, object?[] arguments)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(arguments);
        if (!typeof(IPredicate).IsAssignableFrom(type))
        {
            throw new ArgumentException(
                $"The type '{type.FullName}' does not implement '{nameof(IPredicate)}'.",
                nameof(type));
        }

        var parameters = arguments.Select(argument => argument switch
        {
            IParameter parameter => parameter,
            _ => new LiteralParameter(argument),
        });
        return new SinglePredication(Bindings.Function.FromParameters(type.Name, parameters));
    }

    /// <summary>Represents a non-empty immutable Boolean rule.</summary>
    public sealed class Rule
    {
        private readonly FunctionFactory factory;
        private readonly IPredication predication;

        internal Rule(FunctionFactory factory, IPredication predication)
            => (this.factory, this.predication) = (factory, predication);

        public Rule And<T>()
            where T : IPredicate
            => And(typeof(T));
        public Rule And<T>(params object?[] arguments)
            where T : IPredicate
            => And(typeof(T), arguments);
        public Rule And(Type type, params object?[] arguments) => Combine("And", CreateLeaf(type, arguments));
        public Rule And(Rule rule) => Combine("And", RequireCompatible(rule));

        public Rule Or<T>()
            where T : IPredicate
            => Or(typeof(T));
        public Rule Or<T>(params object?[] arguments)
            where T : IPredicate
            => Or(typeof(T), arguments);
        public Rule Or(Type type, params object?[] arguments) => Combine("Or", CreateLeaf(type, arguments));
        public Rule Or(Rule rule) => Combine("Or", RequireCompatible(rule));

        public Rule Xor<T>()
            where T : IPredicate
            => Xor(typeof(T));
        public Rule Xor<T>(params object?[] arguments)
            where T : IPredicate
            => Xor(typeof(T), arguments);
        public Rule Xor(Type type, params object?[] arguments) => Combine("Xor", CreateLeaf(type, arguments));
        public Rule Xor(Rule rule) => Combine("Xor", RequireCompatible(rule));

        public Rule Not() => new(factory, new UnaryPredication(new UnaryOperator("!"), predication));

        public Predication Build()
            => new(factory.InstantiatePredication(predication));

        public string ToSource() => new PredicationSerializer().Serialize(predication);

        private Rule Combine(string operation, IPredication right)
            => new(factory, new BinaryPredication(new BinaryOperator(operation), predication, right));

        private IPredication RequireCompatible(Rule rule)
        {
            ArgumentNullException.ThrowIfNull(rule);
            if (!ReferenceEquals(factory, rule.factory))
            {
                throw new ArgumentException("Rules must originate from the same Expressif environment.", nameof(rule));
            }

            return rule.predication;
        }
    }
}
