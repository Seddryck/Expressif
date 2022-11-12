using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values.Casters
{
    public class TextCaster
    {
        public string Execute(object obj) => obj.ToString() ?? string.Empty;
    }
}
