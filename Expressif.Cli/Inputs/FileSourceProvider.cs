namespace Expressif.Cli.Inputs;

internal interface IFileSourceProvider
{
    bool CanOpen(string path);

    object? Open(string path, IReadOnlyList<string> options);
}

internal sealed class FileSourceProvider(
    Func<string, bool> canOpen,
    Func<string, IReadOnlyList<string>, object?> open) : IFileSourceProvider
{
    public bool CanOpen(string path) => canOpen(path);

    public object? Open(string path, IReadOnlyList<string> options) => open(path, options);
}
