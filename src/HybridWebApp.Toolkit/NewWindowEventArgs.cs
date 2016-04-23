using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace HybridWebApp.Toolkit
{
    public class NewWindowEventArgs : EventArgs
    {
        private bool _Handled;

        public bool Handled
        {
            get
            {
                if (_EventArgs != null)
                {
                    return _EventArgs.Handled;
                }

                return _Handled;
            }
            set
            {
                if (_EventArgs != null)
                {
                    _EventArgs.Handled = value;
                }
                else
                {
                    _Handled = value;
                }
            }
        }

        public Uri Uri { get; private set; }

        public Uri Referrer { get; private set; }

        private WebViewNewWindowRequestedEventArgs _EventArgs;

        public NewWindowEventArgs(WebViewNewWindowRequestedEventArgs eventArgs)
        {
            _EventArgs = eventArgs;

            this.Uri = eventArgs.Uri;
            this.Referrer = eventArgs.Referrer;
        }

        public NewWindowEventArgs(Uri uri, Uri referrer)
        {
            this.Uri = uri;
            this.Referrer = referrer;
        }

    }
}
