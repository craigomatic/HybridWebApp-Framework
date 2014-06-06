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

        public bool IsSuccess { get; private set; }

        public int WebErrorStatus { get; private set; }

        public WrappedNavigatedEventArgs(Uri uri, bool isSuccess, int webErrorStatus = 0)
        {
            this.Uri = uri;
            this.IsSuccess = isSuccess;
            this.WebErrorStatus = webErrorStatus;
        }
    }
}
