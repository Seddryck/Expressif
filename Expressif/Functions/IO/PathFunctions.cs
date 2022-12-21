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

    /// <summary>
    /// Returns the file name and extension of a file path provided as argument.
    /// </summary>
    class Filename : BasePathFunction
    {
        protected override object EvaluateString(string value) => Path.GetFileName(value);
    }

    /// <summary>
    /// Returns the file name without the extension of a file path provided as argument.
    /// </summary>
    class FilenameWithoutExtension : BasePathFunction
    {
        protected override object EvaluateString(string value) => Path.GetFileNameWithoutExtension(value);
    }

    /// <summary>
    /// Returns the extension of a file path provided as argument.
    /// </summary>
    class Extension : BasePathFunction
    {
        protected override object EvaluateString(string value) => Path.GetExtension(value);
    }

    /// <summary>
    /// Returns the root directory information of a file path provided as argument. Returns `empty` if path does not contain root directory information or is `null`.
    /// </summary>
    class Root : BasePathFunction
    {
        protected override object EvaluateString(string value) => Path.GetPathRoot(value) ?? string.Empty;
    }

    /// <summary>
    /// Returns the directory information of a file path provided as argument. The value is always ending by `/` character. Returns `empty` if path does not contain root directory information or is `null`.
    /// </summary>
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
