using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Expressif.Benchmark;

internal sealed class ExpressionAdapter
{
    private readonly Func<string, object> parseExpression;
    private readonly Func<string, object> createExpression;
    private readonly Func<object, object?, object?> evaluateExpression;

    private ExpressionAdapter(
        Func<string, object> parseExpression,
        Func<string, object> createExpression,
        Func<object, object?, object?> evaluateExpression)
        => (this.parseExpression, this.createExpression, this.evaluateExpression) = (
            parseExpression,
            createExpression,
            evaluateExpression);

    public static ExpressionAdapter Load(string versionDirectory)
    {
        if (!SetDllDirectory(versionDirectory))
            throw new System.ComponentModel.Win32Exception(
                Marshal.GetLastWin32Error(),
                $"Could not add '{versionDirectory}' to the native DLL search path.");

        var name = Path.GetFileName(versionDirectory);
        var kind = name.StartsWith("v1", StringComparison.OrdinalIgnoreCase)
            ? AdapterKind.V1
            : name.StartsWith("v2", StringComparison.OrdinalIgnoreCase)
                ? AdapterKind.V2
                : throw new InvalidOperationException($"Unsupported version folder '{name}'.");
        var assemblyPath = Path.Combine(versionDirectory, "Expressif.dll");
        var context = new VersionLoadContext(assemblyPath);
        var assembly = context.LoadFromAssemblyPath(assemblyPath);

        return kind switch
        {
            AdapterKind.V1 => CreateV1(assembly),
            AdapterKind.V2 => CreateV2(assembly),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
    }

    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetDllDirectory(string pathName);

    public object Create(string source) => createExpression(source);

    public object Parse(string source) => parseExpression(source);

    public Func<object?, object?> CreateEvaluator(string source)
    {
        var expression = createExpression(source);
        return value => evaluateExpression(expression, value);
    }

    private static ExpressionAdapter CreateV1(Assembly assembly)
    {
        var expressionType = RequireType(assembly, "Expressif.Expression");
        var constructor = expressionType.GetConstructor([typeof(string)])
            ?? throw new MissingMethodException(expressionType.FullName, ".ctor(string)");
        return new ExpressionAdapter(
            CompileV1Parser(assembly),
            CompileConstructor(constructor),
            CompileEvaluate(expressionType));
    }

    private static ExpressionAdapter CreateV2(Assembly assembly)
    {
        var implementationType = RequireType(assembly, "Expressif.Expression");
        var create = implementationType.GetMethod(
            "Create",
            BindingFlags.Public | BindingFlags.Static,
            [typeof(string)])
            ?? throw new MissingMethodException(implementationType.FullName, "Create(string)");
        var functionType = RequireType(assembly, "Expressif.Functions.IFunction");
        var parserType = RequireType(assembly, "Expressif.Syntax.ExpressionParser");
        var parse = parserType.GetMethod(
            "Parse",
            BindingFlags.Public | BindingFlags.Static,
            [typeof(string)])
            ?? throw new MissingMethodException(parserType.FullName, "Parse(string)");
        return new ExpressionAdapter(
            CompileStaticCall(parse),
            CompileStaticCall(create),
            CompileEvaluate(functionType));
    }

    private static Type RequireType(Assembly assembly, string typeName)
        => assembly.GetType(typeName, throwOnError: true)!;

    private static Func<string, object> CompileConstructor(ConstructorInfo constructor)
    {
        var source = Expression.Parameter(typeof(string), "source");
        var body = Expression.Convert(Expression.New(constructor, source), typeof(object));
        return Expression.Lambda<Func<string, object>>(body, source).Compile();
    }

    private static Func<string, object> CompileV1Parser(Assembly assembly)
    {
        var rootExpressionType = RequireType(assembly, "Expressif.Parsers.RootExpression");
        var parser = rootExpressionType.GetField("Parser", BindingFlags.Public | BindingFlags.Static)?.GetValue(null)
            ?? throw new MissingFieldException(rootExpressionType.FullName, "Parser");
        var parserType = parser.GetType();
        var parserExtensions = RequireType(parserType.Assembly, "Sprache.ParserExtensions");
        var parse = parserExtensions.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(method => method.Name == "Parse"
                && method.IsGenericMethodDefinition
                && method.GetParameters() is [_, { ParameterType: var parameterType }]
                && parameterType == typeof(string))
            .MakeGenericMethod(parserType.GetGenericArguments()[0]);
        var source = Expression.Parameter(typeof(string), "source");
        var body = Expression.Convert(
            Expression.Call(parse, Expression.Constant(parser, parserType), source),
            typeof(object));
        return Expression.Lambda<Func<string, object>>(body, source).Compile();
    }

    private static Func<string, object> CompileStaticCall(MethodInfo create)
    {
        var source = Expression.Parameter(typeof(string), "source");
        var body = Expression.Convert(Expression.Call(create, source), typeof(object));
        return Expression.Lambda<Func<string, object>>(body, source).Compile();
    }

    private static Func<object, object?, object?> CompileEvaluate(Type expressionType)
    {
        var evaluate = expressionType.GetMethod("Evaluate", [typeof(object)])
            ?? throw new MissingMethodException(expressionType.FullName, "Evaluate(object)");
        var instance = Expression.Parameter(typeof(object), "expression");
        var value = Expression.Parameter(typeof(object), "value");
        var body = Expression.Convert(
            Expression.Call(Expression.Convert(instance, expressionType), evaluate, value),
            typeof(object));
        return Expression.Lambda<Func<object, object?, object?>>(body, instance, value).Compile();
    }
}
