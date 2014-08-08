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
        private IScriptInvoker _ScriptInvoker;

        public Interpreter(IScriptInvoker scriptInvoker)
        {
            _ScriptInvoker = scriptInvoker;
        }

        public async Task LoadFrameworkAsync(WebToHostMessageChannel messageChannel = WebToHostMessageChannel.Default, string baseUri = "http://localhost")
        {
            var scriptPayload = await EmbeddedResource.ReadAsStringAsync(typeof(Interpreter).GetTypeInfo().Assembly, string.Format("{0}.www.js.framework.js", typeof(Interpreter).GetTypeInfo().Namespace));
            await this.EvalAsync(scriptPayload);

            switch(messageChannel)
            {
                case WebToHostMessageChannel.IFrame:
                    {
                        //trim trailing slash as the framework includes it
                        baseUri = baseUri.TrimEnd('/');

                        await this.EvalAsync(string.Format("framework.injectMessageProxy('{0}');", baseUri));
                        break;
                    }
            }
        }

        public void Load(string scriptPayload)
        {
            this.Eval(scriptPayload);
        }

        public async Task LoadAsync(string scriptPayload)
        {
            await this.EvalAsync(scriptPayload);
        }

        public void LoadCss(string cssPayload)
        {
            this.Eval(string.Format("framework.appendCss(\"{0}\");", cssPayload.Replace("\r", string.Empty).Replace("\n", string.Empty)));
        }

        public async Task LoadCssAsync(string cssPayload)
        {
            await this.EvalAsync(string.Format("framework.appendCss(\"{0}\");", cssPayload.Replace("\r", string.Empty).Replace("\n", string.Empty)));
        }

        public string Eval(string scriptPayload)
        {
            return (string)_ScriptInvoker.Eval(scriptPayload);
        }

        public Task<string> EvalAsync(string scriptPayload)
        {
            return _ScriptInvoker.EvalAsync(scriptPayload);
        }
    }
}
