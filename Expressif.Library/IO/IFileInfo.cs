using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Library.IO;

public interface IFileInfo
{
    DateTime LastWriteTimeUtc { get; }
    DateTime LastWriteTime { get; }
    DateTime CreationTimeUtc { get; }
    DateTime CreationTime { get; }
    long Length { get; }
    bool Exists { get; }
}
