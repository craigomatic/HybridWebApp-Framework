using HybridWebApp.Framework.IO;
using HybridWebApp.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HybridWebApp.Framework
{
    public class Interpreter
    {
        private string _Namespace;
        private Assembly _Assembly;

        private IScriptInvoker _ScriptInvoker;

        public Interpreter(IScriptInvoker scriptInvoker, Assembly assembly, string scriptNamespace)
        {
            _ScriptInvoker = scriptInvoker;
            _Assembly = assembly;
            _Namespace = scriptNamespace;
        }

        public async Task LoadFrameworkAsync()
        {
            var scriptPayload = await EmbeddedResource.ReadAsStringAsync(typeof(Interpreter).GetTypeInfo().Assembly, string.Format("{0}.www.js.framework.js", typeof(Interpreter).GetTypeInfo().Namespace));
            this.Eval(scriptPayload);
        }

        public void Load(string scriptName)
        {
            var scriptPayload = EmbeddedResource.ReadAsString(_Assembly, string.Format("{0}.{1}", _Namespace, scriptName));

            this.Eval(scriptPayload);
        }

        public async Task LoadAsync(string scriptName)
        {
            var scriptPayload = await EmbeddedResource.ReadAsStringAsync(_Assembly, string.Format("{0}.{1}", _Namespace, scriptName));

            this.Eval(scriptPayload);
        }

        public string Eval(string scriptPayload)
        {
            try
            {
                return (string)_ScriptInvoker.Eval(scriptPayload);
            }
            catch { }

            return string.Empty;
        }
    }
}
