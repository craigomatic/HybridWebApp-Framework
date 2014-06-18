using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridWebApp.Framework.Model
{
    public class WrappedFailedEventArgs
    {
        public Uri Uri { get; private set; }

        public bool Handled { get; set; }

        public WrappedFailedEventArgs(Uri uri)
        {
            this.Uri = uri;
        }
    }
}
