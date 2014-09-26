﻿using HybridWebApp.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;

namespace HybridWebApp.Toolkit
{
    public class BrowserWrapper : IScriptInvoker, IBrowser
    {
        [Obsolete("Use Navigated instead")]
        public event EventHandler<WrappedNavigatedEventArgs> LoadCompleted;
        [Obsolete("Use Navigated instead")]
        public event EventHandler<WrappedFailedEventArgs> NavigationFailed;
        public event EventHandler<WrappedNavigatingEventArgs> Navigating;
        public event EventHandler<WrappedNavigatedEventArgs> Navigated;

        public event EventHandler<Uri> DOMContentLoaded;

        public WebView WebView { get; private set; }

        public bool CanGoBack
        {
            get { return this.WebView.CanGoBack; }
        }

        public bool CanGoForward
        {
            get { return this.WebView.CanGoForward; }
        }

        public string UserAgent { get; internal set; }

        private Uri _CurrentUri;

        public BrowserWrapper(WebView browser, string userAgent = null)
        {
            this.WebView = browser;

            if (userAgent != null)
            {
                this.UserAgent = userAgent;
            }

            this.WebView.NavigationStarting += WebView_NavigationStarting;
            this.WebView.NavigationCompleted += WebView_NavigationCompleted;
            this.WebView.DOMContentLoaded += WebView_DOMContentLoaded;
        }

        void WebView_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            if(this.DOMContentLoaded != null)
            {
                this.DOMContentLoaded(this, args.Uri);
            }
        }

        void WebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs e)
        {
            if (this.Navigated != null)
            {
                this.Navigated(this, new WrappedNavigatedEventArgs(e.Uri, e.IsSuccess, (int)e.WebErrorStatus));
            }

            if (this.LoadCompleted != null)
            {
                this.LoadCompleted(this, new WrappedNavigatedEventArgs(e.Uri, e.IsSuccess, (int)e.WebErrorStatus));
            }
        }

        void WebView_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            if (this.NavigationFailed != null)
            {
                this.NavigationFailed(this, new WrappedFailedEventArgs(e.Uri));
            }
        }

        void WebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs e)
        {
            //if a custom user agent is in play, cancel the navigation when it has been initiated by the browser as it won't include the custom user-agent
            if (!string.IsNullOrWhiteSpace(this.UserAgent) && e.Uri != _CurrentUri)
            {
                e.Cancel = true;

                this.Navigate(e.Uri);
                return;
            }

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

        public async Task<string> EvalAsync(params string[] args)
        {
            return await this.InvokeAsync("eval", args);
        }

        public async Task<string> InvokeAsync(string scriptName, params string[] args)
        {
            return await this.WebView.InvokeScriptAsync(scriptName, args);
        }

        public void Navigate(Uri uri)
        {
            _CurrentUri = uri;

            if(!string.IsNullOrWhiteSpace(this.UserAgent))
            {
                var httpRequestMessage = new Windows.Web.Http.HttpRequestMessage(Windows.Web.Http.HttpMethod.Get, uri);
                httpRequestMessage.Headers.UserAgent.Add(new Windows.Web.Http.Headers.HttpProductInfoHeaderValue(this.UserAgent));

                this.WebView.NavigateWithHttpRequestMessage(httpRequestMessage);
            }
            else
            {
                this.WebView.Navigate(uri);
            }
        }

        public void Navigate(Uri uri, HttpMethod httpMethod, IList<KeyValuePair<string, string>> httpHeaders, IHttpContent httpContent = null)
        {
            _CurrentUri = uri;

            var httpRequestMessage = new Windows.Web.Http.HttpRequestMessage(httpMethod, uri);

            if (!string.IsNullOrWhiteSpace(this.UserAgent))
            {
                httpRequestMessage.Headers.UserAgent.Add(new Windows.Web.Http.Headers.HttpProductInfoHeaderValue(this.UserAgent));
            }

            if (httpHeaders != null)
            {
                foreach (var item in httpHeaders)
                {
                    httpRequestMessage.Headers.Add(item);
                }
            }

            if (httpContent != null)
            {
                httpRequestMessage.Content = httpContent;
            }

            this.WebView.NavigateWithHttpRequestMessage(httpRequestMessage);
        }

        public void GoBack()
        {
            this.WebView.GoBack();
        }

        public void GoForward()
        {
            this.WebView.GoForward();
        }
    }
}
