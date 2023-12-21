using Expressif.Functions.IO;
using Expressif.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Functions.IO;

[TestFixture]
public class FileFunctionsTest
{
    [Test]
    [TestCase(@"C:\Dir\Child\file.txt", 4080)]
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

    [Test]
    [TestCase(@"C:\Dir\Child\file.txt")]
    public void CreationDateTime_Valid(string value)
    {
        var function = new CreationDateTime();
        IFileInfo Init(string value)
        {
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.SetupGet(x => x.Exists).Returns(true);
            fileInfo.SetupGet(x => x.CreationTime).Returns(new DateTime(2022, 11, 06, 23, 07, 02, DateTimeKind.Local));
            return fileInfo.Object;
        }
        function.SetFileInfoInitializer(Init);
        Assert.That(function.Evaluate(value), Is.EqualTo(new DateTime(2022, 11, 06, 23, 07, 02, DateTimeKind.Local)));
    }

    [Test]
    [TestCase(@"C:\Dir\Child\file.txt")]
    public void CreationDateTimeUtc_Valid(string value)
    {
        var function = new CreationDateTimeUtc();
        IFileInfo Init(string value)
        {
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.SetupGet(x => x.Exists).Returns(true);
            fileInfo.SetupGet(x => x.CreationTimeUtc).Returns(new DateTime(2022, 11, 06, 23, 07, 02, DateTimeKind.Utc));
            return fileInfo.Object;
        };
        function.SetFileInfoInitializer(Init);
        Assert.That(function.Evaluate(value), Is.EqualTo(new DateTime(2022, 11, 06, 23, 07, 02, DateTimeKind.Utc)));
    }

    [Test]
    [TestCase(@"C:\Dir\Child\file.txt")]
    public void UpdateDateTime_Valid(string value)
    {
        var function = new UpdateDateTime();
        IFileInfo Init(string value)
        {
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.SetupGet(x => x.Exists).Returns(true);
            fileInfo.SetupGet(x => x.LastWriteTime).Returns(new DateTime(2022, 11, 06, 23, 07, 02, DateTimeKind.Local));
            return fileInfo.Object;
        }
        function.SetFileInfoInitializer(Init);
        Assert.That(function.Evaluate(value), Is.EqualTo(new DateTime(2022, 11, 06, 23, 07, 02, DateTimeKind.Local)));
    }

    [Test]
    [TestCase(@"C:\Dir\Child\file.txt")]
    public void UpdateDateTimeUtc_Valid(string value)
    {
        var function = new UpdateDateTimeUtc();
        IFileInfo Init(string value)
        {
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.SetupGet(x => x.Exists).Returns(true);
            fileInfo.SetupGet(x => x.LastWriteTimeUtc).Returns(new DateTime(2022, 11, 06, 23, 07, 02, DateTimeKind.Utc));
            return fileInfo.Object;
        }
        function.SetFileInfoInitializer(Init);
        Assert.That(function.Evaluate(value), Is.EqualTo(new DateTime(2022, 11, 06, 23, 07, 02, DateTimeKind.Utc)));
    }
}
