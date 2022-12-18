﻿using Expressif.Functions.Text;
using Expressif.Values.Special;
using System.IO;


namespace Expressif.Functions.IO
{
    [Function(prefix: "file")]
    abstract class BaseFileFunction : BaseTextFunction, IBasePathTransformation
    {
        private Func<string, IFileInfo> FileInfoInitializer { get; set; }
        public BaseFileFunction() => FileInfoInitializer = x => new FileInfoWrapper(x);
        protected override object EvaluateNull() => throw new InvalidIOException(new Null().Keyword);
        protected override object EvaluateEmpty() => throw new InvalidIOException(new Empty().Keyword);
        protected override object EvaluateBlank() => throw new InvalidIOException(new Whitespace().Keyword);
        protected override object EvaluateSpecial(string value) => throw new InvalidIOException("special value");
        protected override object EvaluateString(string value)
        {
            var fileInfo = FileInfoInitializer(value);
            if (!fileInfo.Exists)
                throw new FileNotFoundException(value);
            return EvaluateFileInfo(fileInfo);
        }

        protected abstract object EvaluateFileInfo(IFileInfo value);

        internal void SetFileInfoInitializer(Func<string, IFileInfo> fileInfoInitializer)
            => FileInfoInitializer = fileInfoInitializer;
    }

    class Size : BaseFileFunction
    {
        protected override object EvaluateFileInfo(IFileInfo value) => value.Length;
    }

    class CreationDateTime : BaseFileFunction
    {
        protected override object EvaluateFileInfo(IFileInfo value) => value.CreationTime;
    }

    class CreationDateTimeUtc : BaseFileFunction
    {
        protected override object EvaluateFileInfo(IFileInfo value) => value.CreationTimeUtc;
    }

    class UpdateDateTime : BaseFileFunction
    {
        protected override object EvaluateFileInfo(IFileInfo value) => value.LastWriteTime;
    }

    class UpdateDateTimeUtc : BaseFileFunction
    {
        protected override object EvaluateFileInfo(IFileInfo value) => value.LastWriteTimeUtc;
    }
}