using Expressif.Cli.Application;

namespace Expressif.Cli.Tests;

public class ReplLineEditorTests
{
    [Test]
    public void Read_HistoryNavigation_StopsAtOldestAndMovesForward()
    {
        var editor = new ReplLineEditor();
        _ = Read(editor, Character('a'), Key(ConsoleKey.Enter));
        _ = Read(editor, Character('b'), Key(ConsoleKey.Enter));

        Assert.That(Read(editor, Key(ConsoleKey.UpArrow), Key(ConsoleKey.UpArrow),
            Key(ConsoleKey.UpArrow), Key(ConsoleKey.DownArrow), Key(ConsoleKey.Enter)), Is.EqualTo("b"));
        Assert.That(Read(editor, Key(ConsoleKey.UpArrow), Key(ConsoleKey.UpArrow),
            Key(ConsoleKey.UpArrow), Key(ConsoleKey.Enter)), Is.EqualTo("a"));
    }

    [Test]
    public void Read_DownPastNewest_RestoresDraftAndCursor()
    {
        var editor = new ReplLineEditor();
        _ = Read(editor, Character('a'), Key(ConsoleKey.Enter));

        Assert.That(Read(editor, Character('b'), Character('d'), Key(ConsoleKey.LeftArrow),
            Key(ConsoleKey.UpArrow), Key(ConsoleKey.DownArrow), Key(ConsoleKey.DownArrow),
            Character('c'), Key(ConsoleKey.Enter)), Is.EqualTo("bcd"));
    }

    [Test]
    public void Read_HistoryNavigation_RestoresDraftUndoStack()
    {
        var editor = new ReplLineEditor();
        _ = Read(editor, Character('a'), Key(ConsoleKey.Enter));

        Assert.That(Read(editor, Character('b'), Character('c'), Key(ConsoleKey.UpArrow),
            Key(ConsoleKey.DownArrow), Control(ConsoleKey.Z), Key(ConsoleKey.Enter)), Is.EqualTo("b"));
    }

    [Test]
    public void Read_EditingRecalledInput_DoesNotChangeHistoryAndSupportsUndo()
    {
        var editor = new ReplLineEditor();
        _ = Read(editor, Character('a'), Key(ConsoleKey.Enter));
        Assert.That(Read(editor, Key(ConsoleKey.UpArrow), Character('b'), Control(ConsoleKey.Z),
            Character('c'), Key(ConsoleKey.Enter)), Is.EqualTo("ac"));
        Assert.That(Read(editor, Key(ConsoleKey.UpArrow), Key(ConsoleKey.UpArrow),
            Key(ConsoleKey.Enter)), Is.EqualTo("a"));
    }

    [Test]
    public void Read_BlanksAndConsecutiveDuplicates_AreExcludedFromHistory()
    {
        var editor = new ReplLineEditor();
        _ = Read(editor, Character('a'), Key(ConsoleKey.Enter));
        _ = Read(editor, Character('b'), Key(ConsoleKey.Enter));
        _ = Read(editor, Character('b'), Key(ConsoleKey.Enter));
        _ = Read(editor, Key(ConsoleKey.Enter));
        _ = Read(editor, Character(' '), Key(ConsoleKey.Enter));

        Assert.That(Read(editor, Key(ConsoleKey.UpArrow), Key(ConsoleKey.UpArrow),
            Key(ConsoleKey.Enter)), Is.EqualTo("a"));
    }

    [Test]
    public void Read_EmptyHistory_PreservesInput()
    {
        Assert.That(Read(Character('a'), Key(ConsoleKey.UpArrow), Key(ConsoleKey.DownArrow),
            Key(ConsoleKey.Enter)), Is.EqualTo("a"));
    }

    [Test]
    public void Read_HistoryNavigation_ClearsAllRowsOfWrappedInput()
    {
        var editor = new ReplLineEditor();
        _ = Read(editor, Characters("abcdefghij").Append(Key(ConsoleKey.Enter)).ToArray());
        var output = new StringWriter();

        var result = Read(editor, output, 2, 8,
            Key(ConsoleKey.UpArrow), Key(ConsoleKey.DownArrow),
            Key(ConsoleKey.UpArrow), Key(ConsoleKey.DownArrow), Key(ConsoleKey.Enter));

        const string escape = "\u001b[";
        var recall = $"\r{escape}2C{escape}K\r{escape}2Cabcdefghij";
        var clear = $"\r{escape}1A{escape}2C{escape}K{escape}1B\r{escape}2K{escape}1A\r{escape}2C";
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Empty);
            Assert.That(output.ToString(), Is.EqualTo(recall + clear + recall + clear + Environment.NewLine));
        });
    }

    [Test]
    public void Read_WrappedInput_RestoresCursorToLogicalPosition()
    {
        var output = new StringWriter();

        var result = Read(new ReplLineEditor(), output, 2, 8,
            Characters("abcdefghij")
                .Append(Key(ConsoleKey.Home))
                .Append(Key(ConsoleKey.RightArrow))
                .Append(Character('X'))
                .Append(Key(ConsoleKey.Enter))
                .ToArray());

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo("aXbcdefghij"));
            Assert.That(output.ToString(), Does.Contain("\u001b[3C"));
        });
    }

    [Test]
    public void Read_SingleLineInput_PreservesBackspaceRedraw()
    {
        var output = new StringWriter();

        var result = Read(new ReplLineEditor(), output, 2, 80,
            Character('a'), Character('b'), Key(ConsoleKey.LeftArrow), Character('c'), Key(ConsoleKey.Enter));

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo("acb"));
            Assert.That(output.ToString().Contains('\u001b'), Is.False);
        });
    }

    [Test]
    public void Read_ControlZWhileTyping_UndoesTextEdit()
    {
        Assert.That(Read(Character('a'), Character('b'), Control(ConsoleKey.Z), Key(ConsoleKey.Enter)), Is.EqualTo("a"));
    }

    [Test]
    public void Read_ControlZAfterDeletingInput_ReturnsUndoCommand()
    {
        Assert.That(Read(Character('a'), Key(ConsoleKey.Backspace), Control(ConsoleKey.Z)), Is.EqualTo(":undo"));
    }

    [Test]
    public void Read_NavigationInsertionAndDeletion_PreservesEditedLine()
    {
        Assert.That(Read(
            Character('a'), Character('c'), Key(ConsoleKey.LeftArrow), Character('b'),
            Key(ConsoleKey.Home), Key(ConsoleKey.Delete), Key(ConsoleKey.End), Character('d'),
            Key(ConsoleKey.LeftArrow), Key(ConsoleKey.RightArrow), Key(ConsoleKey.Backspace),
            Key(ConsoleKey.Enter)), Is.EqualTo("bc"));
    }

    [Test]
    public void Read_ControlC_Cancels()
    {
        Assert.That(() => Read(Control(ConsoleKey.C)), Throws.TypeOf<OperationCanceledException>());
    }

    [Test]
    public void Read_CancelledToken_DoesNotReadKey()
    {
        Assert.That(() => new ReplLineEditor().Read(
            _ => throw new AssertionException("Unexpected key read"), TextWriter.Null, new CancellationToken(true)),
            Throws.TypeOf<OperationCanceledException>());
    }

    private static string? Read(params ConsoleKeyInfo[] keys)
        => Read(new ReplLineEditor(), keys);

    private static string? Read(ReplLineEditor editor, params ConsoleKeyInfo[] keys)
    {
        var queue = new Queue<ConsoleKeyInfo>(keys);
        return editor.Read(_ => queue.Dequeue(), TextWriter.Null, CancellationToken.None);
    }

    private static string? Read(
        ReplLineEditor editor,
        TextWriter output,
        int promptWidth,
        int terminalWidth,
        params ConsoleKeyInfo[] keys)
    {
        var queue = new Queue<ConsoleKeyInfo>(keys);
        return editor.Read(_ => queue.Dequeue(), output, CancellationToken.None, promptWidth, terminalWidth);
    }

    private static IEnumerable<ConsoleKeyInfo> Characters(string value) => value.Select(Character);

    private static ConsoleKeyInfo Character(char value) => new(value, ConsoleKey.NoName, false, false, false);

    private static ConsoleKeyInfo Key(ConsoleKey key) => new('\0', key, false, false, false);

    private static ConsoleKeyInfo Control(ConsoleKey key) => new('\0', key, false, false, true);
}
