using System.Text;

namespace Expressif.Cli.Infrastructure;

internal enum TextFileFailureKind
{
    Directory,
    NotFound,
    InvalidUtf8,
    Access,
    Empty,
}

internal sealed class TextFileReadException(string path, TextFileFailureKind kind, Exception? innerException = null)
    : Exception(innerException?.Message, innerException)
{
    public string Path { get; } = path;
    public TextFileFailureKind Kind { get; } = kind;
}

internal interface IStrictUtf8TextReader
{
    string Read(string path, bool requireContent = true);
}

internal sealed class StrictUtf8TextReader : IStrictUtf8TextReader
{
    public string Read(string path, bool requireContent = true)
    {
        if (Directory.Exists(path))
            throw new TextFileReadException(path, TextFileFailureKind.Directory);
        if (!File.Exists(path))
            throw new TextFileReadException(path, TextFileFailureKind.NotFound);

        string text;
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(stream, new UTF8Encoding(false, true), true);
            text = reader.ReadToEnd();
        }
        catch (DecoderFallbackException exception)
        {
            throw new TextFileReadException(path, TextFileFailureKind.InvalidUtf8, exception);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            throw new TextFileReadException(path, TextFileFailureKind.Access, exception);
        }

        if (requireContent && string.IsNullOrWhiteSpace(text))
            throw new TextFileReadException(path, TextFileFailureKind.Empty);
        return text;
    }
}
