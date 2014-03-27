using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridWebApp.Framework.Model
{
    public class WrappedNavigatingEventArgs : EventArgs
    {
        public Uri Uri { get; private set; }

        public bool Cancel { get; set; }

        public WrappedNavigatingEventArgs(Uri uri)
        {
            this.Uri = uri;
        }
    }
}
