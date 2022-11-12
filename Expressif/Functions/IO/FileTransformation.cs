using Expressif.Functions.Text;
using Expressif.Values.Special;
using System.IO;


namespace Expressif.Functions.IO
{
    abstract class AbstractFileTransformation : AbstractTextTransformation, IBasePathTransformation
    {
        protected string BasePath { get; }
        public AbstractFileTransformation(string basePath) => BasePath = basePath;
        protected override object EvaluateNull() => throw new InvalidIOException(new Null().Keyword);
        protected override object EvaluateEmpty() => throw new InvalidIOException(new Empty().Keyword);
        protected override object EvaluateBlank() => throw new InvalidIOException(new Whitespace().Keyword);
        protected override object EvaluateSpecial(string value) => throw new InvalidIOException("special value");
        protected override object EvaluateString(string value)
        {
            var fullPath = Path.Combine(BasePath, value);
            var fileInfo = new FileInfo(fullPath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException(fullPath);
            return EvaluateFileInfo(fileInfo);
        }

        protected abstract object EvaluateFileInfo(FileInfo value);
    }

    class FileToSize : AbstractFileTransformation
    {
        public FileToSize(string basePath) : base(basePath) { }
        protected override object EvaluateFileInfo(FileInfo value) => value.Length;
    }

    class FileToCreationDateTime : AbstractFileTransformation
    {
        public FileToCreationDateTime(string basePath) : base(basePath) { }
        protected override object EvaluateFileInfo(FileInfo value) => value.CreationTime;
    }

    class FileToCreationDateTimeUtc : AbstractFileTransformation
    {
        public FileToCreationDateTimeUtc(string basePath) : base(basePath) { }
        protected override object EvaluateFileInfo(FileInfo value) => value.CreationTimeUtc;
    }

    class FileToUpdateDateTime : AbstractFileTransformation
    {
        public FileToUpdateDateTime(string basePath) : base(basePath) { }
        protected override object EvaluateFileInfo(FileInfo value) => value.LastWriteTime;
    }

    class FileToUpdateDateTimeUtc : AbstractFileTransformation
    {
        public FileToUpdateDateTimeUtc(string basePath) : base(basePath) { }
        protected override object EvaluateFileInfo(FileInfo value) => value.LastWriteTimeUtc;
    }
}
