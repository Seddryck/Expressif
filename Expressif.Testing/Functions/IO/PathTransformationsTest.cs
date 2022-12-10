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
        public void Filename_Valid(string value, string expected)
            => Assert.That(new Filename().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase(@"C:\", "")]
        [TestCase(@"C:\Dir\", "")]
        [TestCase(@"C:\Dir\Child\", "")]
        [TestCase(@"C:\Dir\ChildFile", "ChildFile")]
        [TestCase(@"C:\Dir\Child\file.txt", "file")]
        [TestCase(@"Dir\file.txt", "file")]
        public void FilenameWithoutExtension_Valid(string value, string expected)
            => Assert.That(new FilenameWithoutExtension().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase(@"C:\", "")]
        [TestCase(@"C:\Dir\", "")]
        [TestCase(@"C:\Dir\Child\", "")]
        [TestCase(@"C:\Dir\ChildFile", "")]
        [TestCase(@"C:\Dir\Child\file.txt", ".txt")]
        [TestCase(@"Dir\file.txt", @".txt")]
        public void Extension_Valid(string value, string expected)
            => Assert.That(new Extension().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase(@"C:\", @"C:\")]
        [TestCase(@"C:\Dir\", @"C:\")]
        [TestCase(@"C:\Dir\Child\", @"C:\")]
        [TestCase(@"C:\Dir\ChildFile", @"C:\")]
        [TestCase(@"C:\Dir\Child\file.txt", @"C:\")]
        [TestCase(@"Dir\file.txt", @"")]
        public void Root_Valid(string value, string expected)
            => Assert.That(new Root().Evaluate(value), Is.EqualTo(expected));

        [Test]
        [TestCase(@"C:\", @"C:\")]
        [TestCase(@"C:\Dir\", @"C:\Dir\")]
        [TestCase(@"C:\Dir\Child\", @"C:\Dir\Child\")]
        [TestCase(@"C:\Dir\ChildFile", @"C:\Dir\")]
        [TestCase(@"C:\Dir\Child\file.txt", @"C:\Dir\Child\")]
        [TestCase(@"Dir\file.txt", @"Dir\")]
        public void Directory_Valid(string value, string expected)
            => Assert.That(new Expressif.Functions.IO.Directory().Evaluate(value), Is.EqualTo(expected));
    }
}
