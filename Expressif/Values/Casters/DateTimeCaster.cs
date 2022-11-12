using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values.Casters
{
    public class DateTimeCaster
    {
        public DateTime Execute(object obj) => Convert.ToDateTime(obj, CultureInfo.InvariantCulture.DateTimeFormat);
    }
}
