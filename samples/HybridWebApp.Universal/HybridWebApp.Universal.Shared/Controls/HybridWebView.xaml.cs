﻿using HybridWebApp.Framework;
using HybridWebApp.Framework.Model;
using HybridWebApp.Toolkit;
using System;
using System.Reflection;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HybridWebApp.Universal.Controls
{
    public sealed partial class HybridWebView : UserControl
    {
        public event TypedEventHandler<HybridWebView, ScriptMessage> MessageReceived;

        public event TypedEventHandler<HybridWebView, Uri> NavigationStarting;

        public event TypedEventHandler<HybridWebView, Uri> DOMContentLoaded;

        public string WebUri
        {
            get { return (string)GetValue(WebUriProperty); }
            set { SetValue(WebUriProperty, value); }
        }

        public static readonly DependencyProperty WebUriProperty =
            DependencyProperty.Register("WebUri", typeof(string), typeof(HybridWebView), new PropertyMetadata(string.Empty));

        private Interpreter _Interpreter;
        private WebRoute _WebRoute;
        private BrowserWrapper _BrowserWrapper;

        public HybridWebView()
        {
            this.InitializeComponent();

            this.Loaded += HybridWebView_Loaded;
        }

        void HybridWebView_Loaded(object sender, RoutedEventArgs e)
        {            
            _BrowserWrapper = new BrowserWrapper(WebView);
            _Interpreter = new Interpreter(_BrowserWrapper, typeof(HybridWebView).GetTypeInfo().Assembly, "HybridWebApp.Universal", "HybridWebApp.Universal");
            _WebRoute = new WebRoute(new Uri(this.WebUri), _Interpreter, _BrowserWrapper);

            _WebRoute.Map("/", async (uri, success, errorCode) =>
            {
                if (success)
                {
                    await _Interpreter.LoadFrameworkAsync(WebToHostMessageChannel.IFrame, this.WebUri);
                    await _Interpreter.LoadAsync("app.js");
                    await _Interpreter.LoadCssAsync("app.css");

                    _HideNavigatingOverlay();
                }
                else
                {
                    _HideNavigatingOverlay();
                    _ShowOfflineOverlay();
                }
            });

            _WebRoute.MapOtherHosts(async uri => 
            {
                await Windows.System.Launcher.LaunchUriAsync(uri);
            });

            //only load the menu once
            _WebRoute.Map("/", async (uri, success, errorCode) =>
            {
                await _Interpreter.EvalAsync("app.readMenu();");
            }, true);

            WebView.FrameNavigationStarting += async (s, args) =>
            {
                await _ProcessMessageAsync(args.Uri);

                //cancel navigation
                args.Cancel = true;
            };

            WebView.Navigate(_WebRoute.Root);
        }

        public void Navigate(Uri uri)
        {
            if(_WebRoute.CurrentUri == uri)
            {
                return;
            }

            _BrowserWrapper.Navigate(uri);
        }

        private void _OnMessageReceived(ScriptMessage scriptMessage)
        {
            if (MessageReceived != null)
            {
                MessageReceived(this, scriptMessage);
            }
        }

        private async Task _ProcessMessageAsync(Uri uri)
        {
            if (!uri.ToString().Contains(string.Format("{0}/hwaf/", this.WebUri.TrimEnd('/'))) || uri.Segments.Length < 3)
            {
                return;
            }

            //process message
            var encodedMsg = uri.AbsolutePath.Replace("/hwaf/", string.Empty);
            var jsonString = System.Net.WebUtility.UrlDecode(encodedMsg).Replace("/\"", "\\\"");
            var msg = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ScriptMessage>(jsonString));

            //send it up to be handled
            _OnMessageReceived(msg);
        }

        #region Overlays

        private void _ShowNavigatingOverlay()
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            OfflineOverlay.Visibility = Visibility.Collapsed;

            //Browser.Opacity = 0.1d;
            WebView.Visibility = Visibility.Visible;
        }

        private void _HideNavigatingOverlay()
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
            OfflineOverlay.Visibility = Visibility.Collapsed;

            //Browser.Opacity = 1;
            WebView.Visibility = Visibility.Visible;
        }

        private void _ShowOfflineOverlay()
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
            OfflineOverlay.Visibility = Visibility.Visible;

            WebView.Visibility = Visibility.Collapsed;//.Opacity = 0.1d;
        }

        /// <summary>
        /// Hides the offline overlay and transitions to the navigating overlay before refreshing the browser
        /// </summary>
        private void _HideOfflineOverlayAndRetry()
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            OfflineOverlay.Visibility = Visibility.Collapsed;

            _BrowserWrapper.Navigate(_WebRoute.CurrentUri);
        }

        #endregion

        private void WebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if (this.NavigationStarting != null)
            {
                this.NavigationStarting(this, args.Uri);
            }

            _ShowNavigatingOverlay();
        }

        private void WebView_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            if (this.DOMContentLoaded != null)
            {
                this.DOMContentLoaded(this, args.Uri);
            }
        }
    }
}
