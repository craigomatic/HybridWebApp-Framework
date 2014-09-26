using HybridWebApp.Framework;
using HybridWebApp.Framework.Model;
using HybridWebApp.Toolkit;
using System;
using System.Reflection;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.Web.Http;
using System.Collections.Generic;

namespace HybridWebApp.Toolkit.Controls
{
    public sealed partial class HybridWebView : UserControl
    {
        /// <summary>
        /// Raised when a new Script Message is received from the web page
        /// </summary>
        public event TypedEventHandler<HybridWebView, ScriptMessage> MessageReceived;

        public event TypedEventHandler<HybridWebView, Uri> NavigationStarting;

        public event TypedEventHandler<HybridWebView, Uri> DOMContentLoaded;

        /// <summary>
        /// Fires when the HybridWebView is ready for manipulation
        /// </summary>
        public event EventHandler Ready;

        public Brush LoadingBackgroundBrush { get; set; }

        public string LoadingBackgroundImage { get; set; }

        public double LoadingBackgroundImageWidth { get; set; }

        public Brush OfflineBackgroundBrush { get; set; }

        public string OfflineBackgroundImage { get; set; }

        public double OfflineBackgroundImageWidth { get; set; }

        /// <summary>
        /// Default CSS resource to load
        /// </summary>
        public string CssResourcePath { get; set; }

        /// <summary>
        /// Additional CSS for devices that identify as a phone
        /// </summary>
        public string CssResourcePathPhone { get; set; }
        
        /// <summary>
        /// Additional CSS for devices that don't identify as a phone
        /// </summary>
        public string CssResourcePathNotPhone { get; set; }

        /// <summary>
        /// Default JS resource to load
        /// </summary>
        public string JsResourcePath { get; set; }

        /// <summary>
        /// Gets or sets the base website URI the control should render.
        /// </summary>
        public Uri WebUri { get; set; }

        /// <summary>
        /// When true, the WebUri is navigated to when the control finishes loading
        /// </summary>
        public bool NavigateOnLoad { get; set; }

        /// <summary>
        /// When true, the default loading overlay is displayed during navigation requests. 
        /// </summary>
        /// <remarks>For single-page apps this should almost always be set to false as they typically provide their own loading spinners.</remarks>
        public bool EnableLoadingOverlay { get; set; }

        /// <summary>
        /// When true, the default offline overlay is displayed when navigation attempts fail due to connectivity
        /// </summary>
        public bool EnableOfflineOverlay { get; set; }

        /// <summary>
        /// When true, all requests do a host/path combination that differs from the WebUri will be opened in an external IE window
        /// </summary>
        public bool OpenOtherHostsExternal { get; set; }

        /// <summary>
        /// Gets or sets the custom user agent that will be sent out with each request (doesn't update the client-side userAgent retrieved via Javascript)
        /// </summary>
        public string UserAgent
        {
            get { return _BrowserWrapper.UserAgent; }
            set { _BrowserWrapper.UserAgent = value; }
        }

        private Interpreter _Interpreter;

        /// <summary>
        /// Gets the interpreter used to load JS/CSS and run arbitrary JS
        /// </summary>
        public Interpreter Interpreter
        {
            get { return _Interpreter; }
        }
        
        private WebRoute _WebRoute;

        public WebRoute WebRoute
        {
            get { return _WebRoute; }
        }

        public bool CanGoBack
        {
            get { return WebView.CanGoBack; }
        }

        public bool CanGoForward
        {
            get { return WebView.CanGoForward; }
        }        

        private BrowserWrapper _BrowserWrapper;

        private bool _IsInitialised;

        public HybridWebView()
        {
            this.InitializeComponent();

            this.LoadingBackgroundImageWidth = 360;
            this.OfflineBackgroundImageWidth = 360;

            _BrowserWrapper = new BrowserWrapper(WebView);
            _Interpreter = new Interpreter(_BrowserWrapper);
            _WebRoute = new WebRoute(_Interpreter, _BrowserWrapper);

            this.OpenOtherHostsExternal = true;
            this.NavigateOnLoad = true;
            this.EnableLoadingOverlay = true;
            this.EnableOfflineOverlay = true;

            OfflineOverlay.RetryAction = () => { this.HideOfflineOverlayAndRetry(); };
        }

        async void HybridWebView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_IsInitialised)
            {
                await _Initialise();
            }

            if (this.Ready != null)
            {
                this.Ready(this, EventArgs.Empty);
            }

            if (this.NavigateOnLoad && _WebRoute.Root != null)
            {
                _BrowserWrapper.Navigate(_WebRoute.Root);
            }
        }

        private async Task _Initialise()
        {
            //cache the CSS and JS 
            var cssString = string.Empty;

            if (!string.IsNullOrWhiteSpace(this.CssResourcePath))
            {
                try
                {
                    var cssFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(this.CssResourcePath));
                    cssString = await Windows.Storage.FileIO.ReadTextAsync(cssFile);
                }
                catch
                {
                    throw new Exception(string.Format("Unable to load CSS from the given path {0}.", this.CssResourcePath));
                }
            }

            var jsString = string.Empty;

            if (!string.IsNullOrWhiteSpace(this.JsResourcePath))
            {
                try
                {
                    var jsFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(this.JsResourcePath));
                    jsString = await Windows.Storage.FileIO.ReadTextAsync(jsFile);
                }
                catch
                {
                    throw new Exception(string.Format("Unable to load JS from the given path {0}.", this.JsResourcePath));
                }
            }

            var additionalPhoneCssString = string.Empty;

            if (!string.IsNullOrWhiteSpace(this.CssResourcePathPhone))
            {
                try
                {
                    var additionalCssFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(this.CssResourcePathPhone));
                    additionalPhoneCssString = await Windows.Storage.FileIO.ReadTextAsync(additionalCssFile);
                }
                catch
                {
                    throw new Exception(string.Format("Unable to load CSS from the given path {0}.", this.CssResourcePathPhone));
                }
            }

            var additionalNotPhoneCssString = string.Empty;

            if (!string.IsNullOrWhiteSpace(this.CssResourcePathNotPhone))
            {
                try
                {
                    var additionalCssFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(this.CssResourcePathNotPhone));
                    additionalNotPhoneCssString = await Windows.Storage.FileIO.ReadTextAsync(additionalCssFile);
                }
                catch
                {
                    throw new Exception(string.Format("Unable to load CSS from the given path {0}.", this.CssResourcePathNotPhone));
                }
            }

            _WebRoute.Root = this.WebUri;

            _WebRoute.Map("/", async (uri, success, errorCode) =>
            {
                if (success)
                {
                    await _Interpreter.LoadFrameworkAsync(WebToHostMessageChannel.IFrame);

                    //load JS
                    if (!string.IsNullOrWhiteSpace(jsString))
                    {
                        await _Interpreter.LoadAsync(jsString);
                    }

                    //load CSS
                    if (!string.IsNullOrWhiteSpace(cssString))
                    {
                        await _Interpreter.LoadCssAsync(cssString);
                    }

                    if (!string.IsNullOrWhiteSpace(additionalPhoneCssString))
                    {
                        await _Interpreter.LoadCssAsync(additionalPhoneCssString);
                    }

                    if (!string.IsNullOrWhiteSpace(additionalNotPhoneCssString))
                    {
                        await _Interpreter.LoadCssAsync(additionalNotPhoneCssString);
                    }

                    _HideNavigatingOverlay();
                }
                else
                {
                    _HideNavigatingOverlay();
                    _ShowOfflineOverlay();
                }
            });

            if (this.OpenOtherHostsExternal)
            {
                _WebRoute.MapOtherHosts(async uri =>
                {
                    _HideNavigatingOverlay();
                    await Windows.System.Launcher.LaunchUriAsync(uri);
                });
            }

            WebView.FrameNavigationStarting += async (s, args) =>
            {
                await _ProcessMessageAsync(args.Uri);

                //cancel navigation
                args.Cancel = true;
            };

            _IsInitialised = true;
        }

        public void Navigate(Uri uri)
        {
            if(_WebRoute.CurrentUri == uri)
            {
                return;
            }

            _BrowserWrapper.Navigate(uri);
        }

        public void Navigate(Uri uri, HttpMethod httpMethod, IList<KeyValuePair<string, string>> httpHeaders, IHttpContent httpContent = null)
        {
            if (_WebRoute.CurrentUri == uri)
            {
                return;
            }

            _BrowserWrapper.Navigate(uri, httpMethod, httpHeaders, httpContent);
        }

        public void GoBack()
        {
            WebView.GoBack();
        }

        public void GoForward()
        {
            WebView.GoForward();
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
            if (!uri.OriginalString.Contains(FrameworkConstants.MessageProxyPath) || uri.OriginalString.EndsWith(FrameworkConstants.MessageProxyPath))
            {
                return;
            }

            //process message
            var encodedMsg = uri.AbsolutePath.Replace(FrameworkConstants.MessageProxyPath, string.Empty);
            var jsonString = System.Net.WebUtility.UrlDecode(encodedMsg).Replace("/\"", "\\\"");
            var msg = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ScriptMessage>(jsonString));

            //send it up to be handled
            _OnMessageReceived(msg);
        }

        #region Overlays

        private void _ShowNavigatingOverlay()
        {
            if (!this.EnableLoadingOverlay)
            {
                return;
            }

            this.ShowOfflineOverlay();
        }

        public void ShowNavigatingOverlay()
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            OfflineOverlay.Visibility = Visibility.Collapsed;

            //Browser.Opacity = 0.1d;
            WebView.Visibility = Visibility.Visible;
        }

        private void _HideNavigatingOverlay()
        {
            if (!this.EnableLoadingOverlay)
            {
                return;
            }

            this.HideNavigatingOverlay();
        }

        public void HideNavigatingOverlay()
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
            OfflineOverlay.Visibility = Visibility.Collapsed;

            //Browser.Opacity = 1;
            WebView.Visibility = Visibility.Visible;
        }

        private void _ShowOfflineOverlay()
        {
            if (!this.EnableOfflineOverlay)
            {
                return;
            }

            this.ShowOfflineOverlay();
        }

        public void ShowOfflineOverlay()
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
            OfflineOverlay.Visibility = Visibility.Visible;

            WebView.Visibility = Visibility.Collapsed;//.Opacity = 0.1d;
        }

        private void _HideOfflineOverlayAndRetry()
        {
            if (this.EnableLoadingOverlay)
            {
                LoadingOverlay.Visibility = Visibility.Visible;
            }

            if (this.EnableOfflineOverlay)
            {
                OfflineOverlay.Visibility = Visibility.Collapsed;
            }

            _BrowserWrapper.Navigate(_WebRoute.CurrentUri);
        }

        /// <summary>
        /// Hides the offline overlay and transitions to the navigating overlay before refreshing the browser
        /// </summary>
        public void HideOfflineOverlayAndRetry()
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
