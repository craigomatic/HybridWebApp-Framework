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
        private string _ScriptNamespace;
        private string _CssNamespace;
        private Assembly _Assembly;

        private IScriptInvoker _ScriptInvoker;

        public Interpreter(IScriptInvoker scriptInvoker, Assembly assembly, string scriptNamespace, string cssNamespace)
        {
            _ScriptInvoker = scriptInvoker;
            _Assembly = assembly;
            _ScriptNamespace = scriptNamespace;
            _CssNamespace = cssNamespace;
        }

        public async Task LoadFrameworkAsync(bool overrideWindowExternalNotify = false)
        {
            var scriptPayload = await EmbeddedResource.ReadAsStringAsync(typeof(Interpreter).GetTypeInfo().Assembly, string.Format("{0}.www.js.framework.js", typeof(Interpreter).GetTypeInfo().Namespace));
            this.Eval(scriptPayload);

            if (overrideWindowExternalNotify)
            {
                this.Eval("framework.injectMessageProxy();");
            }
        }

        public void Load(string scriptName)
        {
            var scriptPayload = EmbeddedResource.ReadAsString(_Assembly, string.Format("{0}.{1}", _ScriptNamespace, scriptName));

            this.Eval(scriptPayload);
        }

        public async Task LoadAsync(string scriptName)
        {
            var scriptPayload = await EmbeddedResource.ReadAsStringAsync(_Assembly, string.Format("{0}.{1}", _ScriptNamespace, scriptName));

            this.Eval(scriptPayload);
        }

        public void LoadCss(string cssName)
        {
            var cssPayload = EmbeddedResource.ReadAsString(_Assembly, string.Format("{0}.{1}", _CssNamespace, cssName));

            this.Eval(string.Format("framework.appendCss(\"{0}\");", cssPayload.Replace("\r", string.Empty).Replace("\n", string.Empty)));
        }

        public async Task LoadCssAsync(string cssName)
        {
            var cssPayload = await EmbeddedResource.ReadAsStringAsync(_Assembly, string.Format("{0}.{1}", _CssNamespace, cssName));

            this.Eval(string.Format("framework.appendCss(\"{0}\");", cssPayload.Replace("\r", string.Empty).Replace("\n", string.Empty)));
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
