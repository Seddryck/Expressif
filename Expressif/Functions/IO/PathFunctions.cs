using Expressif.Functions.Text;
using Expressif.Values.Special;
using System;
using System.IO;

namespace Expressif.Functions.IO
{
    [Function(prefix: "path")]
    abstract class BasePathFunction : BaseTextFunction, IBasePathTransformation
    {
        public BasePathFunction() { }
        protected override object EvaluateNull() => new Empty().Keyword;
        protected override object EvaluateEmpty() => new Empty().Keyword;
        protected override object EvaluateBlank() => new Empty().Keyword;
        protected override object EvaluateSpecial(string value) => new Empty().Keyword;
    }

    class Filename : BasePathFunction
    {
        protected override object EvaluateString(string value) => Path.GetFileName(value);
    }

    class FilenameWithoutExtension : BasePathFunction
    {
        protected override object EvaluateString(string value) => Path.GetFileNameWithoutExtension(value);
    }

    class Extension : BasePathFunction
    {
        protected override object EvaluateString(string value) => Path.GetExtension(value);
    }

    class Root : BasePathFunction
    {
        protected override object EvaluateString(string value) => Path.GetPathRoot(value) ?? string.Empty;
    }

    class Directory : BasePathFunction
    {
        protected override object EvaluateString(string value)
        {
            return Path.GetDirectoryName(value) == null 
                ? Path.GetPathRoot(value) ?? string.Empty 
                : Path.GetDirectoryName(value) + Path.DirectorySeparatorChar;
        }
            
    }
}
