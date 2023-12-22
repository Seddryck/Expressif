using Expressif.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif;

public class Context
{
    public ContextVariables Variables { get; } = new ();
    public ContextObject CurrentObject { get; } = new ();
}
