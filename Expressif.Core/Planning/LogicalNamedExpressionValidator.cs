namespace Expressif.Planning;

internal static class LogicalNamedExpressionValidator
{
    public static void Validate(LogicalPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var definitions = plan.Definitions ?? throw Error("The named-expression definition collection is missing.");
        var byName = new Dictionary<string, LogicalNamedExpressionDefinition>(StringComparer.Ordinal);
        foreach (var definition in definitions)
        {
            if (definition is null || string.IsNullOrWhiteSpace(definition.Name))
                throw Error("A named-expression definition must have a name.");
            if (!byName.TryAdd(definition.Name, definition))
                throw Error($"Named expression '{definition.Name}' is declared more than once.");
            ValidateDefinition(definition);
        }

        foreach (var invocation in EnumerateInvocations(plan.Pipeline)
            .Concat(definitions.SelectMany(definition => EnumerateInvocations(definition.Body))))
        {
            if (!byName.TryGetValue(invocation.Name, out var target))
                throw Error($"Named expression '{invocation.Name}' is not defined.");
            var parameters = target.EffectiveParameters;
            var required = parameters.Count(parameter => parameter.Default is null);
            if (invocation.Arguments.Count < required || invocation.Arguments.Count > parameters.Count)
            {
                throw Error(
                    $"Named expression '{invocation.Name}' expects between {required} and {parameters.Count} arguments "
                    + $"but received {invocation.Arguments.Count}.");
            }
        }

        ValidateCycles(byName);
    }

    private static void ValidateDefinition(LogicalNamedExpressionDefinition definition)
    {
        if (definition.Body is null || definition.Body.Items is null || definition.Body.Items.Count == 0)
            throw Error($"Named expression '{definition.Name}' must contain a non-empty body pipeline.");

        var names = new HashSet<string>(StringComparer.Ordinal);
        var optional = false;
        foreach (var receiver in definition.EffectiveReceivers)
        {
            if (receiver is null)
                throw Error($"Named expression '{definition.Name}' contains a null receiver descriptor.");
            ValidateName(receiver.Name, definition.Name, names);
            ValidateContract(receiver.Contract, definition.Name, receiver.Name);
        }
        foreach (var parameter in definition.EffectiveParameters)
        {
            if (parameter is null)
                throw Error($"Named expression '{definition.Name}' contains a null parameter descriptor.");
            ValidateName(parameter.Name, definition.Name, names);
            if (parameter.Default is not null)
                optional = true;
            else if (optional)
                throw Error($"Required parameter '{parameter.Name}' cannot follow an optional parameter on '{definition.Name}'.");
            ValidateContract(parameter.Contract, definition.Name, parameter.Name);
        }
        ValidateContract(definition.InputContract, definition.Name, "input");
        ValidateContract(definition.OutputContract, definition.Name, "output");
    }

    private static void ValidateName(string name, string definition, ISet<string> names)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw Error($"Named expression '{definition}' contains an empty local name.");
        if (!names.Add(name))
            throw Error($"Named expression '{definition}' declares local name '{name}' more than once.");
    }

    private static void ValidateContract(LogicalTypeContract? contract, string definition, string boundary)
    {
        if (contract is not null && string.IsNullOrWhiteSpace(contract.Type))
            throw Error($"Named expression '{definition}' has an empty type contract for '{boundary}'.");
    }

    private static void ValidateCycles(IReadOnlyDictionary<string, LogicalNamedExpressionDefinition> definitions)
    {
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var path = new List<string>();
        foreach (var name in definitions.Keys)
            Visit(name);

        void Visit(string name)
        {
            if (visited.Contains(name))
                return;
            if (!visiting.Add(name))
            {
                var start = path.IndexOf(name);
                var cycle = path.Skip(start).Append(name);
                throw Error($"Named-expression dependency cycle: {string.Join(" -> ", cycle)}.");
            }

            path.Add(name);
            foreach (var dependency in EnumerateInvocations(definitions[name].Body)
                .Select(invocation => invocation.Name)
                .Distinct(StringComparer.Ordinal))
            {
                Visit(dependency);
            }
            path.RemoveAt(path.Count - 1);
            visiting.Remove(name);
            visited.Add(name);
        }
    }

    internal static IEnumerable<LogicalNamedExpressionInvocation> EnumerateInvocations(LogicalValue value)
    {
        switch (value)
        {
            case LogicalNamedExpressionInvocation invocation:
                if (string.IsNullOrWhiteSpace(invocation.Name))
                    throw Error("A named-expression invocation must identify its target.");
                if (invocation.Arguments is null)
                    throw Error($"Named expression '{invocation.Name}' has no argument collection.");
                yield return invocation;
                foreach (var nested in invocation.Arguments.SelectMany(EnumerateInvocations))
                    yield return nested;
                break;
            case LogicalPipeline pipeline:
                foreach (var nested in pipeline.Items.SelectMany(EnumerateInvocations))
                    yield return nested;
                break;
            case LogicalCall call:
                foreach (var nested in call.Arguments
                    .Where(argument => argument.Value is not null)
                    .SelectMany(argument => EnumerateInvocations(argument.Value!)))
                {
                    yield return nested;
                }
                break;
        }
    }

    private static LogicalPlanFormatException Error(string message) => new(message);
}
