using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridWebApp.Framework.Model
{
    public interface IScriptInvoker
    {
        Task<string> EvalAsync(params string[] args);

        Task<string> InvokeAsync(string scriptName, params string[] args);
    }
}
