using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridWebApp.Framework.Model
{
    public interface IScriptInvoker
    {
        object Eval(params string[] args);

        object Invoke(string scriptName, params string[] args);
    }
}
