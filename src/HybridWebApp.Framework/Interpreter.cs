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

        public async Task LoadFrameworkAsync(WebToHostMessageChannel messageChannel = WebToHostMessageChannel.Default)
        {
            var scriptPayload = await EmbeddedResource.ReadAsStringAsync(typeof(Interpreter).GetTypeInfo().Assembly, string.Format("{0}.www.js.framework.js", typeof(Interpreter).GetTypeInfo().Namespace));
            await this.EvalAsync(scriptPayload);

            switch(messageChannel)
            {
                case WebToHostMessageChannel.IFrame:
                    {
                        await this.EvalAsync("framework.injectMessageProxy();");
                        break;
                    }
            }
        }
        
        public async Task LoadAsync(string scriptPayload)
        {
            await this.EvalAsync(scriptPayload);
        }

        public async Task LoadCssAsync(string cssPayload)
        {
            await this.EvalAsync(string.Format("framework.appendCss(\"{0}\");", cssPayload.Replace("\r", string.Empty).Replace("\n", string.Empty)));
        }
        
        public Task<string> EvalAsync(string scriptPayload)
        {
            return _ScriptInvoker.EvalAsync(scriptPayload);
        }
    }
}
