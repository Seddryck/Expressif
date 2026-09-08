using Expressif.Cli.Application;

namespace Expressif.Cli.Tests;

public class ReplLineEditorTests
{
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
    {
        var queue = new Queue<ConsoleKeyInfo>(keys);
        return new ReplLineEditor().Read(_ => queue.Dequeue(), TextWriter.Null, CancellationToken.None);
    }

    private static ConsoleKeyInfo Character(char value) => new(value, ConsoleKey.NoName, false, false, false);

    private static ConsoleKeyInfo Key(ConsoleKey key) => new('\0', key, false, false, false);

    private static ConsoleKeyInfo Control(ConsoleKey key) => new('\0', key, false, false, true);
}
