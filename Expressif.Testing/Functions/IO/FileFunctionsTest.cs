using Expressif.Functions.IO;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.IO;

[TestFixture]
public class FileFunctionsTest
{
    [Conformance]
    public void Size_Valid(string value, long expected)
    {
        var function = new Size();
        IFileInfo Init(string value)
        {
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.SetupGet(x => x.Exists).Returns(true);
            fileInfo.SetupGet(x => x.Length).Returns(4080);
            return fileInfo.Object;
        }
        function.SetFileInfoInitializer(Init);
        Assert.That(function.Evaluate(value), Is.EqualTo(expected));
    }

    [Conformance]
    public void CreationDateTime_Valid(string value, DateTime expected)
    {
        expected = new DateTime(expected.Ticks, DateTimeKind.Local);

        var function = new CreationDateTime();
        IFileInfo Init(string value)
        {
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.SetupGet(x => x.Exists).Returns(true);
            fileInfo.SetupGet(x => x.CreationTime).Returns(expected);
            return fileInfo.Object;
        }
        function.SetFileInfoInitializer(Init);
        Assert.That(function.Evaluate(value), Is.EqualTo(expected));
    }

    [Conformance]
    public void CreationDateTimeUtc_Valid(string value, DateTime expected)
    {
        expected = new DateTime(expected.Ticks, DateTimeKind.Utc);

        var function = new CreationDateTimeUtc();
        IFileInfo Init(string value)
        {
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.SetupGet(x => x.Exists).Returns(true);
            fileInfo.SetupGet(x => x.CreationTimeUtc).Returns(expected);
            return fileInfo.Object;
        };
        function.SetFileInfoInitializer(Init);
        Assert.That(function.Evaluate(value), Is.EqualTo(expected));
    }

    [Conformance]
    public void UpdateDateTime_Valid(string value, DateTime expected)
    {
        expected = new DateTime(expected.Ticks, DateTimeKind.Local);

        var function = new UpdateDateTime();
        IFileInfo Init(string value)
        {
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.SetupGet(x => x.Exists).Returns(true);
            fileInfo.SetupGet(x => x.LastWriteTime).Returns(expected);
            return fileInfo.Object;
        }
        function.SetFileInfoInitializer(Init);
        Assert.That(function.Evaluate(value), Is.EqualTo(expected));
    }

    [Conformance]
    public void UpdateDateTimeUtc_Valid(string value, DateTime expected)
    {
        expected = new DateTime(expected.Ticks, DateTimeKind.Utc);

        var function = new UpdateDateTimeUtc();
        IFileInfo Init(string value)
        {
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.SetupGet(x => x.Exists).Returns(true);
            fileInfo.SetupGet(x => x.LastWriteTimeUtc).Returns(expected);
            return fileInfo.Object;
        }
        function.SetFileInfoInitializer(Init);
        Assert.That(function.Evaluate(value), Is.EqualTo(expected));
    }
}
