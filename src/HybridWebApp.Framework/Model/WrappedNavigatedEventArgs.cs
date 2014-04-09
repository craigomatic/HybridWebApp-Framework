using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridWebApp.Framework.Model
{
    public class WrappedNavigatedEventArgs : EventArgs
    {
        public Uri Uri { get; private set; }

        public WrappedNavigatedEventArgs(Uri uri)
        {
            this.Uri = uri;
        }
    }
}
