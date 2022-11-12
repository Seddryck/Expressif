using Expressif.Functions.IO;
using Expressif.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Functions.IO
{
    [TestFixture]
    public class PathTransformationsTest
    { 
        [Test]
        [TestCase(@"C:\", "")]
        [TestCase(@"C:\Dir\", "")]
        [TestCase(@"C:\Dir\Child\", "")]
        [TestCase(@"C:\Dir\ChildFile", "ChildFile")]
        [TestCase(@"C:\Dir\Child\file.txt", "file.txt")]
        [TestCase(@"Dir\file.txt", "file.txt")]
        public void PathToFilename_Valid(string value, string expected)
            => Assert.That(new PathToFilename().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase(@"C:\", "")]
        [TestCase(@"C:\Dir\", "")]
        [TestCase(@"C:\Dir\Child\", "")]
        [TestCase(@"C:\Dir\ChildFile", "ChildFile")]
        [TestCase(@"C:\Dir\Child\file.txt", "file")]
        [TestCase(@"Dir\file.txt", "file")]
        public void PathToFilenameWithoutExtension_Valid(string value, string expected)
            => Assert.That(new PathToFilenameWithoutExtension().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase(@"C:\", "")]
        [TestCase(@"C:\Dir\", "")]
        [TestCase(@"C:\Dir\Child\", "")]
        [TestCase(@"C:\Dir\ChildFile", "")]
        [TestCase(@"C:\Dir\Child\file.txt", ".txt")]
        [TestCase(@"Dir\file.txt", @".txt")]
        public void PathToExtension_Valid(string value, string expected)
            => Assert.That(new PathToExtension().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase(@"C:\", @"C:\")]
        [TestCase(@"C:\Dir\", @"C:\")]
        [TestCase(@"C:\Dir\Child\", @"C:\")]
        [TestCase(@"C:\Dir\ChildFile", @"C:\")]
        [TestCase(@"C:\Dir\Child\file.txt", @"C:\")]
        [TestCase(@"Dir\file.txt", @"")]
        public void PathToRoot_Valid(string value, string expected)
            => Assert.That(new PathToRoot().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase(@"C:\", @"C:\")]
        [TestCase(@"C:\Dir\", @"C:\Dir\")]
        [TestCase(@"C:\Dir\Child\", @"C:\Dir\Child\")]
        [TestCase(@"C:\Dir\ChildFile", @"C:\Dir\")]
        [TestCase(@"C:\Dir\Child\file.txt", @"C:\Dir\Child\")]
        [TestCase(@"Dir\file.txt", @"Dir\")]
        public void PathToDirectory_Valid(string value, string expected)
            => Assert.That(new PathToDirectory().Evaluate(value), Is.EqualTo(expected));
    }
}
