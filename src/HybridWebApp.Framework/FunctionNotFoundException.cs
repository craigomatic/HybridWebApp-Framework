using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridWebApp.Framework
{
    public class FunctionNotFoundException : Exception
    {
        public FunctionNotFoundException(string message)
            : base(message)
        { }
    }
}
