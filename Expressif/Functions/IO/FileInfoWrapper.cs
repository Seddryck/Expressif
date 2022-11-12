using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions.IO
{
    internal class FileInfoWrapper : IFileInfo
    {
        private FileInfo FileInfo { get;  }

        public FileInfoWrapper(string value) => FileInfo = new FileInfo(value);

        public DateTime LastWriteTimeUtc => FileInfo.LastWriteTimeUtc;

        public DateTime LastWriteTime => FileInfo.LastWriteTime;

        public DateTime CreationTimeUtc => FileInfo.CreationTimeUtc;

        public DateTime CreationTime => FileInfo.CreationTime;

        public long Length => FileInfo.Length;

        public bool Exists => FileInfo.Exists;
    }
}
