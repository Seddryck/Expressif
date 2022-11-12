using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values.Casters
{
    public class NumericCaster
    {
        public decimal Execute(object obj) => Convert.ToDecimal(obj, CultureInfo.InvariantCulture.NumberFormat);
    }
}
