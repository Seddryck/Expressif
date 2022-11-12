using Expressif.Functions.Text;
using Expressif.Values.Special;
using System;
using System.IO;

namespace Expressif.Functions.IO
{
    abstract class AbstractPathTransformation : AbstractTextTransformation, IBasePathTransformation
    {
        protected string BasePath { get; }
        public AbstractPathTransformation(string basePath) => BasePath = basePath;
        protected override object EvaluateNull() => new Empty().Keyword;
        protected override object EvaluateEmpty() => new Empty().Keyword;
        protected override object EvaluateBlank() => new Empty().Keyword;
        protected override object EvaluateSpecial(string value) => new Empty().Keyword;
    }

    class PathToFilename : AbstractPathTransformation
    {
        public PathToFilename(string basePath) : base(basePath) { }
        protected override object EvaluateString(string value) => Path.GetFileName(value);
    }

    class PathToFilenameWithoutExtension : AbstractPathTransformation
    {
        public PathToFilenameWithoutExtension(string basePath) : base(basePath) { }
        protected override object EvaluateString(string value) => Path.GetFileNameWithoutExtension(value);
    }

    class PathToExtension : AbstractPathTransformation
    {
        public PathToExtension(string basePath) : base(basePath) { }
        protected override object EvaluateString(string value) => Path.GetExtension(value);
    }

    class PathToRoot : AbstractPathTransformation
    {
        public PathToRoot(string basePath) : base(basePath) { }
        protected override object EvaluateString(string value)
            //=> Path.GetPathRoot(PathExtensions.CombineOrRoot(BasePath, value));
            => string.Empty;
    }

    class PathToDirectory : AbstractPathTransformation
    {
        public PathToDirectory(string basePath) : base(basePath) { }
        protected override object EvaluateString(string value)
        {
            var fullPath = (Path.IsPathRooted(value) || string.IsNullOrEmpty(BasePath))
                ? value
                : Path.Combine(BasePath, value);
            return Path.GetDirectoryName(fullPath) == null 
                ? Path.GetPathRoot(fullPath) ?? string.Empty 
                : Path.GetDirectoryName(fullPath) + Path.DirectorySeparatorChar;
        }
            
    }
}
