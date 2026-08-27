namespace Expressif.Functions;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class ScopeAttribute : Attribute
{
    public string Name { get; }

    public ScopeAttribute(string name)
        => Name = name;
}
