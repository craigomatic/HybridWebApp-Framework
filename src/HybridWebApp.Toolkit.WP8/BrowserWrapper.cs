using HybridWebApp.Framework.Model;
using Microsoft.Phone.Controls;
using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace HybridWebApp.Toolkit.WP8
{
    public class BrowserWrapper : IScriptInvoker, IBrowser
    {
        public event EventHandler<Uri> LoadCompleted;
        public event EventHandler<Uri> NavigationFailed;
        public event EventHandler<WrappedNavigatingEventArgs> Navigating;
        public event EventHandler<WrappedNavigatedEventArgs> Navigated;

        public WebBrowser WebBrowser { get; private set; }

        public BrowserWrapper(WebBrowser browser)
        {
            this.WebBrowser = browser;
            this.WebBrowser.LoadCompleted += WebBrowser_LoadCompleted;
            this.WebBrowser.NavigationFailed += WebBrowser_NavigationFailed;
            this.WebBrowser.Navigating += WebBrowser_Navigating;
            this.WebBrowser.Navigated += WebBrowser_Navigated;
        }

        void WebBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (this.Navigated != null)
            {
                this.Navigated(this, new WrappedNavigatedEventArgs(e.Uri));
            }
        }

        void WebBrowser_Navigating(object sender, NavigatingEventArgs e)
        {
            if (this.Navigating != null)
            {
                var eventArgs = new WrappedNavigatingEventArgs(e.Uri);
                this.Navigating(this, eventArgs);

                e.Cancel = eventArgs.Cancel;
            }
        }

        void WebBrowser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (this.LoadCompleted != null)
            {
                this.LoadCompleted(this, e.Uri);
            }
        }

        void WebBrowser_NavigationFailed(object sender, System.Windows.Navigation.NavigationFailedEventArgs e)
        {
            if (this.NavigationFailed != null)
            {
                this.NavigationFailed(this, e.Uri);
            }
        }

        public object Eval(params string[] args)
        {
            return this.Invoke("eval", args);
        }

        public object Invoke(string scriptName, params string[] args)
        {
            return this.WebBrowser.InvokeScript(scriptName, args);
        }
    }
}
