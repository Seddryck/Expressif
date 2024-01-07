using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values;
public interface IReadOnlyDataRow
{
    int ColumnsCount { get; }
    bool ContainsColumn(string columnName);
    object? this[string columnName] { get; }
    object? this[int index] { get; }
}
