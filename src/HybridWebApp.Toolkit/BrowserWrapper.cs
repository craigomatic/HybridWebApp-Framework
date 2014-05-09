using HybridWebApp.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.UI.Xaml.Controls;

namespace HybridWebApp.Toolkit
{
    public class BrowserWrapper : IScriptInvoker, IBrowser
    {
        public event EventHandler<Uri> LoadCompleted;
        public event EventHandler<Uri> NavigationFailed;
        public event EventHandler<WrappedNavigatingEventArgs> Navigating;
        public event EventHandler<WrappedNavigatedEventArgs> Navigated;

        public WebView WebView { get; private set; }

        public BrowserWrapper(WebView browser)
        {
            this.WebView = browser;
            this.WebView.LoadCompleted += WebView_LoadCompleted;
            this.WebView.NavigationFailed += WebView_NavigationFailed;
            this.WebView.NavigationStarting += WebView_NavigationStarting;
            this.WebView.NavigationCompleted += WebView_NavigationCompleted;
        }

        void WebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs e)
        {
            if (this.Navigated != null)
            {
                this.Navigated(this, new WrappedNavigatedEventArgs(e.Uri));
            }
        }

        void WebView_LoadCompleted(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            if (this.LoadCompleted != null)
            {
                this.LoadCompleted(this, e.Uri);
            }
        }

        void WebView_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            if (this.NavigationFailed != null)
            {
                this.NavigationFailed(this, e.Uri);
            }
        }

        void WebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs e)
        {
            if (this.Navigating != null)
            {
                var eventArgs = new WrappedNavigatingEventArgs(e.Uri);
                this.Navigating(this, eventArgs);

                e.Cancel = eventArgs.Cancel;
            }
        }

        public object Eval(params string[] args)
        {
            return this.Invoke("eval", args);
        }

        public object Invoke(string scriptName, params string[] args)
        {
            return this.WebView.InvokeScript(scriptName, args);
        }
    }
}
