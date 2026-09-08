namespace Expressif.Cli.Application;

internal sealed class ReplLineEditor
{
    public string? Read(
        Func<CancellationToken, ConsoleKeyInfo> readKey,
        TextWriter output,
        CancellationToken cancellationToken)
    {
        var text = string.Empty;
        var cursor = 0;
        var edits = new Stack<(string Text, int Cursor)>();
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var key = readKey(cancellationToken);
            var control = key.Modifiers.HasFlag(ConsoleModifiers.Control);
            if (control && key.Key == ConsoleKey.C)
                throw new OperationCanceledException(cancellationToken);
            if (key.Key == ConsoleKey.Enter || (control && key.Key == ConsoleKey.Z && text.Length == 0))
            {
                output.WriteLine();
                return key.Key == ConsoleKey.Enter ? text : ":undo";
            }

            if (control && key.Key == ConsoleKey.D && text.Length == 0)
            {
                output.WriteLine();
                return null;
            }

            var previousText = text;
            var previousCursor = cursor;
            if (control && key.Key == ConsoleKey.Z)
            {
                if (edits.TryPop(out var previous))
                    (text, cursor) = previous;
            }
            else if (key.Key == ConsoleKey.LeftArrow)
            {
                cursor = Math.Max(0, cursor - 1);
            }
            else if (key.Key == ConsoleKey.RightArrow)
            {
                cursor = Math.Min(text.Length, cursor + 1);
            }
            else if (key.Key == ConsoleKey.Home)
            {
                cursor = 0;
            }
            else if (key.Key == ConsoleKey.End)
            {
                cursor = text.Length;
            }
            else
            {
                if (key.Key == ConsoleKey.Backspace && cursor > 0)
                    text = text.Remove(--cursor, 1);
                else if (key.Key == ConsoleKey.Delete && cursor < text.Length)
                    text = text.Remove(cursor, 1);
                else if (!char.IsControl(key.KeyChar))
                    text = text.Insert(cursor++, key.KeyChar.ToString());

                if (text != previousText)
                    edits.Push((previousText, previousCursor));
            }

            output.Write(new string('\b', previousCursor));
            output.Write(text);
            var padding = Math.Max(0, previousText.Length - text.Length);
            output.Write(new string(' ', padding));
            output.Write(new string('\b', text.Length + padding - cursor));
        }
    }
}
